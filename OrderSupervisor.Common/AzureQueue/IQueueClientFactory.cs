using Azure.Storage.Queues;

namespace OrderSupervisor.Common.AzureQueue
{
    public interface IQueueClientFactory
    {
        QueueClient GetQueueClient();
    }
}
