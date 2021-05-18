using System;

namespace OrderSupervisor.Common.AzureQueue
{
    public class ExponentialRetryIntervalGenerator : IRetryIntervalGenerator
    {
        public TimeSpan GetNext(int dequeueCount)
        {
            return TimeSpan.FromSeconds(dequeueCount * dequeueCount);
        }
    }
}
