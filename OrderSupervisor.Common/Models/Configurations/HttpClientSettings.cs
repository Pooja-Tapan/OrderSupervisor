using System;

namespace OrderSupervisor.Common.Models.Configurations
{
    public class HttpClientSettings
    {
        public Uri BaseUrl { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}
