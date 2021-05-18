using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace OrderSupervisorProcessor.Health
{
    public class ReadyHealthCheckResponseWriter
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public ReadyHealthCheckResponseWriter(IOptions<MvcNewtonsoftJsonOptions> mvcJsonOptions)
        {
            jsonSerializerSettings = mvcJsonOptions.Value.SerializerSettings;
        }

        public async Task WriteResponseAsync(HttpContext httpContext, HealthReport report)
        {
            httpContext.Response.ContentType = "application/json;charset=utf-8";

            var response = "{}";

            if (report != null) response = JsonConvert.SerializeObject(report, jsonSerializerSettings);

            var bytes = Encoding.UTF8.GetBytes(response);
            await httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
