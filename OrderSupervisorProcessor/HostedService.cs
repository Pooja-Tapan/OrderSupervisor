using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.OrderDto;
using OrderSupervisor.Common.Repositories;

namespace OrderSupervisorProcessor
{
    public class HostedService : IHostedService
    {
        private readonly int retryLimit = 5;
        private readonly int maximumBackoff = 10;
        private int currentBackoff = 0;        
        private bool startLoop = true;

        private QueueMessage retryQueueMessage;
        private readonly CancellationTokenSource hostTokenSource;
        private readonly IOrderSupervisorApiClient orderSupervisorApiClient;
        private readonly QueueClient queueClient;
        private readonly TelemetryClient telemetryClient;
        private readonly IRetryIntervalGenerator retryIntervalGenerator;

        public HostedService(HostedServiceMetadata hostedServiceMetadata)
        {
            queueClient = hostedServiceMetadata.QueueClientFactory.GetQueueClient();
            orderSupervisorApiClient = hostedServiceMetadata.OrderSupervisorApiClient;
            telemetryClient = hostedServiceMetadata.TelemetryClient;
            retryIntervalGenerator = hostedServiceMetadata.RetryIntervalGenerator;
            hostTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Register callback to close client if start is cancelled
            cancellationToken.Register(() =>
            {
                hostTokenSource.Cancel();
                queueClient.DeleteIfExistsAsync();
            });

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask; //Exit gracefully
            }

            _= ProcessMessageAsync(cancellationToken);

            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(CancellationToken cancellationToken)
        {
            var agentId = new Guid();
            Random random = new Random();
            var magicNumber = random.Next(10);

            Console.WriteLine($"I’m agent {agentId}, my magic number is {magicNumber}");

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

                            // "Process" the message
                            Console.WriteLine($"Received Order: {message.Order.OrderId}");

                            if (magicNumber == message.Order.RandomNumber)
                            {
                                Console.WriteLine($"Oh no, my magic number was found.");
                                startLoop = false;
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"Displaying Order information: {message.Order.OrderText}");

                                //MakeApi call to store the "processed" order status in "Confirmations" table.
                                var orderConfirmation = new Confirmation
                                {
                                    OrderId = message.Order.OrderId,
                                    AgentId = agentId,
                                    OrderStatus = "Processed"
                                };

                                var result = await orderSupervisorApiClient.SaveOrderProcessStatusAsync(orderConfirmation);

                                // Let the service know we have processed the message and
                                // it can be safely deleted.
                                if (result.Result.Status)
                                {
                                    await queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt);
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
                telemetryClient.TrackException(exception);

                //Implementing retry mechanism for messages not processed due to temporary error.
                await queueClient.UpdateMessageAsync(retryQueueMessage.MessageId, retryQueueMessage.PopReceipt, retryQueueMessage.Body,
                            retryIntervalGenerator.GetNext(Convert.ToInt32(retryQueueMessage.DequeueCount)), cancellationToken);

                if (retryQueueMessage.DequeueCount >= retryLimit)
                {
                    Console.WriteLine("Could not process the message after 5 retries. Sending message to poison queue");
                    //TODO - Send message to poison queue and delete the message
                    //routePoisonMessage(retryQueueMessage);

                    //Delete the message so that it does not reappear on the queue
                    await queueClient .DeleteMessageAsync(retryQueueMessage.MessageId, retryQueueMessage.PopReceipt);
                }
            }
            Console.ReadKey();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            hostTokenSource.Cancel();
            //No need to handle the cancellation token, only one operation is performed
            return queueClient.DeleteIfExistsAsync();
        }
    }
}
