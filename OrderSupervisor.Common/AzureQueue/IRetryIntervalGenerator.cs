using System;

namespace OrderSupervisor.Common.AzureQueue
{
    public interface IRetryIntervalGenerator
    {
        TimeSpan GetNext(int dequeueCount);
    }
}
