using Microsoft.AspNetCore.Mvc;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisor.Common.Models;
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
        private readonly IQueueOperations<EnqueueMessage> queueOperations;

        public OrderSupervisorController(IQueueOperations<EnqueueMessage> queueOperations)
        {
            this.queueOperations = queueOperations;
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
                enqueueMessageRequest.RandomNumber = random.Next(10);
                Console.WriteLine($"Send order #{enqueueMessageRequest.OrderId} with random number {enqueueMessageRequest.RandomNumber}");

                await queueOperations.PublishAsync(enqueueMessageRequest);

                return Accepted();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }               
    }
}
