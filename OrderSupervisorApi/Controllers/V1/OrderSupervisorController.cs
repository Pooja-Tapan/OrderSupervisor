using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.Message;
using System;
using System.Threading.Tasks;

namespace OrderSupervisorApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/Orders")]
    //[Microsoft.AspNetCore.Authorization.Authorize]
    public class OrderSupervisorController : Controller
    {
        private IMemoryCache memoryCache;
        private readonly IQueueOperations<Order> queueOperations;

        public OrderSupervisorController(IQueueOperations<Order> queueOperations, IMemoryCache memoryCache)
        {
            this.queueOperations = queueOperations;
            this.memoryCache = memoryCache;
        }

        [HttpPost]
        [ProducesResponseType(202)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [Route("Enqueue")]
        public async Task<ActionResult> EnqueueMessage([FromBody] EnqueueMessage enqueueMessageRequest)
        {
            try
            {
                Random random = new Random();
                enqueueMessageRequest.Order.RandomNumber = random.Next(10);
                Console.WriteLine($"Send order #{enqueueMessageRequest.Order.OrderId} with random number {enqueueMessageRequest.Order.RandomNumber}");

                await queueOperations.PublishAsync(enqueueMessageRequest.Order);

                return Accepted();
            }
            catch (Exception)
            {
                throw;
            }
        }               
    }
}
