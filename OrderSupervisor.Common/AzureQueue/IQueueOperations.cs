using System.Threading.Tasks;

namespace OrderSupervisor.Common.AzureQueue
{
    public interface IQueueOperations<in T>
    {
        Task PublishAsync(T content);
    }
}
