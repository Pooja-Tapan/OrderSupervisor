using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace OrderSupervisor.Common.Models.OrderDto
{
    public class Confirmation : TableEntity
    {
        public int OrderId { get; set; }
        public Guid AgentId { get; set; }
        public string OrderStatus { get; set; }
    }
}
