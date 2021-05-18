namespace OrderSupervisor.Common.Models
{
    public class OrderRequest
    {
        public int OrderId { get; set; }
        public string PartitionKey { get; set; }
    }
}
