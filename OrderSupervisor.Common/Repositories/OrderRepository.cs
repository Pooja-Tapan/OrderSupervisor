using Microsoft.WindowsAzure.Storage.Table;
using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.OrderDto;
using System.Threading.Tasks;

namespace OrderSupervisor.Common.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CloudTable table;

        public OrderRepository(ICloudTableClient cloudTableClient)
        {
            table = cloudTableClient.GetCloudTable();
        }

        public async Task<Confirmation> GetOrderConfirmationAsync(OrderRequest orderRequest)
        {
            var operation = TableOperation.Retrieve<Confirmation>(orderRequest.PartitionKey, orderRequest.OrderId.ToString());
            var result = await table.ExecuteAsync(operation);
            return result.Result as Confirmation;
        }

        public async Task<Result> SaveOrderProcessStatusAsync(Confirmation confirmationStatus)
        {
            var operation = TableOperation.InsertOrReplace(confirmationStatus);
            var response = await table.ExecuteAsync(operation);

            if (null != response.Result)
            {
                return new Result { Status = true };
            }
            else
            {
                return new Result { Status = false };
            }
        }
    }
}
