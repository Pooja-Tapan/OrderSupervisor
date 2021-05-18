using OrderSupervisor.Common.Models.Message;

namespace OrderSupervisor.Common.Models
{
    public class EnqueueMessage
    {
        public Order Order { get; set; }
    }
}
