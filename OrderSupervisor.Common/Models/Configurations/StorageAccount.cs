namespace OrderSupervisor.Common.Models
{
    public class StorageAccount
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public string TableName { get; set; }
    }
}