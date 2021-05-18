using Newtonsoft.Json;
using System.Threading.Tasks;

namespace OrderSupervisor.Common.AzureQueue
{
    public class QueueOperations<T> : IQueueOperations<T>
    {
        private readonly IQueueClientFactory queueClientFactory;

        public QueueOperations(IQueueClientFactory queueClientFactory)
        {
            this.queueClientFactory = queueClientFactory;
        }

        public async Task PublishAsync(T content)
        {
            var serializedMessage = JsonConvert.SerializeObject(content);
            var queueClient = queueClientFactory.GetQueueClient();
            await queueClient.SendMessageAsync(serializedMessage);
        }
    }
}
