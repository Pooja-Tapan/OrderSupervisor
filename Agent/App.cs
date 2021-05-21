using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.ConfirmationDto;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    class App
    {
        private readonly int retryLimit = 5;
        private readonly int maximumBackoff = 10;
        private Guid agentId;
        private int magicNumber;
        private int currentBackoff = 0;
        private bool startLoop = true;
        private QueueMessage retryQueueMessage;

        private readonly CancellationTokenSource hostTokenSource;
        private readonly QueueClient queueClient;
        private readonly CloudStorageAccount cloudStorageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable table;
        private readonly IRetryIntervalGenerator retryIntervalGenerator;
        
        public App(IConfiguration config, IRetryIntervalGenerator retryIntervalGenerator)
        {
            this.retryIntervalGenerator = retryIntervalGenerator;
            hostTokenSource = new CancellationTokenSource();            
            var connString = config.GetValue<string>("StorageAccount:ConnectionString");
            var queueName = config.GetValue<string>("StorageAccount:QueueName");
            var tableName = config.GetValue<string>("StorageAccount:TableName");

            QueueClientOptions queueClientOptions = new QueueClientOptions()
            {
                MessageEncoding = QueueMessageEncoding.Base64
            };
            queueClient = new QueueClient(connString, queueName, queueClientOptions);
            cloudStorageAccount = CloudStorageAccount.Parse(connString);
            tableClient = cloudStorageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExistsAsync();
        }

        public void Run(CancellationToken cancellationToken)
        {
            //Register callback to close client if run is cancelled
            cancellationToken.Register(() =>
            {
                hostTokenSource.Cancel();
                queueClient.DeleteIfExistsAsync();
            });

            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Canceling...");
                return; //Exit gracefully
            }            

            //Generate new agentId.
            agentId = Guid.NewGuid();
            Random random = new Random();
            magicNumber = random.Next(10);

            Console.WriteLine($"I’m agent {agentId}, my magic number is {magicNumber}");
            
            ProcessMessage(cancellationToken);
        }

        private void ProcessMessage(CancellationToken cancellationToken)
        {
            try
            {
                //starting for the infinite polling of messages in queue.
                while (startLoop)
                {
                    QueueMessage[] retrievedMessage = queueClient.ReceiveMessagesAsync().GetAwaiter().GetResult();

                    //Check if messages are present in queue else implement backoff exponential sleep for worker process.
                    if (retrievedMessage.Length > 0)
                    {
                        // Reset backoff
                        currentBackoff = 0;

                        foreach (QueueMessage queueMessage in retrievedMessage)
                        {
                            var message = JsonConvert.DeserializeObject<EnqueueMessage>(Encoding.UTF8.GetString(queueMessage.Body));

                            retryQueueMessage = queueMessage;
                            if (queueMessage.DequeueCount >= retryLimit)
                            {
                                Console.WriteLine("Could not process the message beyond retry limits. Sending message to poison queue");
                                //TODO - Send message to poison queue and delete the message
                                //routePoisonMessage(retryQueueMessage);

                                //Delete the message so that it does not reappear on the queue
                                queueClient.DeleteMessageAsync(retryQueueMessage.MessageId, retryQueueMessage.PopReceipt);
                                continue;
                            }

                            // "Process" the message
                            Console.WriteLine($"Received Order: {message.OrderId}");

                            if (magicNumber == message.RandomNumber)
                            {
                                Console.WriteLine($"Oh no, my magic number was found.");
                                startLoop = false;
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"Displaying Order information: {message.OrderText}");

                                //MakeApi call to store the "processed" order status in "Confirmations" table.
                                var orderConfirmation = new Confirmation
                                {
                                    OrderId = message.OrderId,
                                    AgentId = agentId,
                                    OrderStatus = "Processed"
                                };

                                var result = SaveOrderProcessConfirmationStatusAsync(orderConfirmation);

                                // Let the service know we have processed the message and
                                // it can be safely deleted.
                                if (result.Result.Status)
                                {
                                    queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentBackoff < maximumBackoff)
                        {
                            currentBackoff++;
                        }
                        Console.WriteLine("Backing off for {0} seconds...", currentBackoff);
                        Thread.Sleep(TimeSpan.FromSeconds(currentBackoff));
                    }
                }
            }
            catch (Exception exception)
            {
                if (retryQueueMessage != null)
                {
                    //Implementing retry mechanism for messages not processed due to temporary error.
                    queueClient.UpdateMessageAsync(retryQueueMessage.MessageId, retryQueueMessage.PopReceipt, retryQueueMessage.Body,
                                retryIntervalGenerator.GetNext(Convert.ToInt32(retryQueueMessage.DequeueCount)), cancellationToken);

                    if (retryQueueMessage.DequeueCount >= retryLimit)
                    {
                        Console.WriteLine("Could not process the message beyond retry limits. Sending message to poison queue");
                        //TODO - Send message to poison queue and delete the message
                        //routePoisonMessage(retryQueueMessage);

                        //Delete the message so that it does not reappear on the queue
                        queueClient.DeleteMessageAsync(retryQueueMessage.MessageId, retryQueueMessage.PopReceipt);
                    }
                }
            }
            Console.ReadKey();
        }

        private async Task<Result> SaveOrderProcessConfirmationStatusAsync(Confirmation orderConfirmation)
        {
            Confirmation confirmationEntity = new Confirmation(orderConfirmation.AgentId, orderConfirmation.OrderId)
            {
                OrderId = orderConfirmation.OrderId,
                OrderStatus = orderConfirmation.OrderStatus,
                AgentId = orderConfirmation.AgentId
            };

            var operation = TableOperation.InsertOrReplace(confirmationEntity);
            var response = await table.ExecuteAsync(operation);

            if (null != response.Result)
            {
                return new Result { Status = true };
            }
            else
            {
                return new Result { Status = false };
            }
        }
    }
}
