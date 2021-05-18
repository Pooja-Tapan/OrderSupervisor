using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.OrderDto;
using System.Threading.Tasks;

namespace OrderSupervisor.Common.Repositories
{
    public interface IOrderRepository
    {
        Task<Confirmation> GetOrderConfirmationAsync(OrderRequest orderRequest);
        Task<Result> SaveOrderProcessStatusAsync(Confirmation confirmationStatus);
    }
}
