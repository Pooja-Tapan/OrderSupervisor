using Microsoft.ApplicationInsights;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisor.Common.Repositories;

namespace OrderSupervisor.Common.Models
{
    public class HostedServiceMetadata
    {
        public IQueueClientFactory QueueClientFactory { get; set; }
        public IOrderSupervisorApiClient OrderSupervisorApiClient { get; set; }
        public IRetryIntervalGenerator RetryIntervalGenerator { get; set; }
        public TelemetryClient TelemetryClient { get; set; }
    }
}