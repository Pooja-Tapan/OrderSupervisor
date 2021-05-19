using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace OrderSupervisor.Common.Models.ConfirmationDto
{
    public class Confirmation : TableEntity
    {
        public Confirmation(Guid AgentId, int OrderId)
        {
            this.PartitionKey = Convert.ToString(AgentId); this.RowKey = Convert.ToString(OrderId);
        }
        public Confirmation() { }
        public int OrderId { get; set; }
        public Guid AgentId { get; set; }
        public string OrderStatus { get; set; }
    }
}
