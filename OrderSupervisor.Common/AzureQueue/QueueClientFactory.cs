using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using OrderSupervisor.Common.Models;

namespace OrderSupervisor.Common.AzureQueue
{
    public class QueueClientFactory : IQueueClientFactory
    {
        private readonly string connectionString;
        private readonly string queueName;

        public QueueClientFactory(IOptions<StorageAccount> storageAccount)
        {
            connectionString = storageAccount.Value.ConnectionString;
            queueName = storageAccount.Value.QueueName;
        }

        public QueueClient GetQueueClient()
        {
            QueueClientOptions queueClientOptions = new QueueClientOptions()
            {
                MessageEncoding = QueueMessageEncoding.Base64                 
            };
            var queueClient = new QueueClient(connectionString, queueName, queueClientOptions);

            return queueClient;
        }
    }
}
