using System.Threading.Tasks;
using Refit;
using OrderSupervisor.Common.Models.OrderDto;
using OrderSupervisor.Common.Models.Api;

namespace OrderSupervisor.Common.Repositories
{
    public interface IOrderSupervisorApiClient
    {
        [Post("/api/v1/confirmation/status")]
        Task<OrderSupervisorApiResponse> SaveOrderProcessStatusAsync([Body] Confirmation confirmationStatus);
    }
}
