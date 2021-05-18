using Microsoft.AspNetCore.Mvc;
using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.OrderDto;
using OrderSupervisor.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace OrderSupervisorApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/Confirmation")]
    //[Microsoft.AspNetCore.Authorization.Authorize]
    public class OrderConfirmationController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        public OrderConfirmationController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [Route("OrderDetail/{orderId}")]
        public async Task<ActionResult<Confirmation>> GetOrder([FromRoute] OrderRequest orderRequest)
        {
            try
            {
                var result = await orderRepository.GetOrderConfirmationAsync(orderRequest);
                if (result == null)
                {
                    return NotFound();
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [Route("Status")]
        public async Task<ActionResult<Result>> SaveOrderProcessStatus([FromBody] Confirmation confirmationStatus)
        {
            var response = await orderRepository.SaveOrderProcessStatusAsync(confirmationStatus);

            return response;
        }
    }
}
