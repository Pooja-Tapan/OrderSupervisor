using System.Threading.Tasks;
using Refit;
using OrderSupervisor.Common.Models.ConfirmationDto;
using OrderSupervisor.Common.Models.Api;

namespace OrderSupervisor.Common.Repositories
{
    public interface IOrderSupervisorApiClient
    {
        [Post("/api/v1/Confirmation/Status")]
        Task<OrderSupervisorApiResponse> SaveOrderProcessStatusAsync([Body] Confirmation confirmationStatus);
    }
}
