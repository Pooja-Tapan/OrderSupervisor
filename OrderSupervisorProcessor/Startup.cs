using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisorProcessor.Health;
using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.Configurations;
using OrderSupervisor.Common.Repositories;
using Refit;
using System;

namespace OrderSupervisorProcessor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var orderSupervisorApiClientSettings = Configuration.GetSection("OrderSupervisorApiClientSettings")
                                                                  .Get<HttpClientSettings>();
            ConfigureRefitServices<IOrderSupervisorApiClient>(services, orderSupervisorApiClientSettings);

            //Healthy status check for Api.
            var orderSupervisorApiHealthUrl = new UriBuilder(orderSupervisorApiClientSettings.BaseUrl)
            {
                Port = 80,
                Path = "health/live"
            };

            services.AddControllers();
            services.AddOptions();
            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks()
                    .AddUrlGroup(orderSupervisorApiHealthUrl.Uri, "Order Supervisor API", HealthStatus.Unhealthy)
                    .AddApplicationInsightsPublisher();

            services.Configure<StorageAccount>(Configuration.GetSection("StorageAccount"));

            services.AddSingleton<ReadyHealthCheckResponseWriter>();
            services.AddHostedService<HostedService>();
            services.AddSingleton<IQueueClientFactory, QueueClientFactory>();
            services.AddSingleton<IRetryIntervalGenerator, ExponentialRetryIntervalGenerator>();

            services.AddSingleton(x => new HostedServiceMetadata()
            {
                QueueClientFactory = x.GetService<IQueueClientFactory>(),
                OrderSupervisorApiClient = x.GetService<IOrderSupervisorApiClient>(),
                TelemetryClient = x.GetService<TelemetryClient>(),
                RetryIntervalGenerator = x.GetService<IRetryIntervalGenerator>()
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ReadyHealthCheckResponseWriter responseWriter)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    ResponseWriter = responseWriter.WriteResponseAsync
                })
                         .RequireHost("*:8421");
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = (_) => false
                });
            });
        }

        private void ConfigureRefitServices<T>(IServiceCollection services, HttpClientSettings settings) where T : class
        {
            var userAgent = $"Order Supervisor Service/{typeof(Startup).Assembly.GetName().Version}";

            var refitSettings = new RefitSettings()
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                })              
            };

            services.AddRefitClient<T>(refitSettings)
                        .ConfigureHttpClient(c =>
                        {
                            c.BaseAddress = settings.BaseUrl;
                            c.Timeout = settings.Timeout;
                            c.DefaultRequestHeaders.Add("Accept", "application/json");
                            c.DefaultRequestHeaders.Add("User-Agent", userAgent);
                        });
        }
    }
}
