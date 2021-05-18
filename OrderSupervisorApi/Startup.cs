using Azure.Storage.Queues;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisor.Common.Models;
using OrderSupervisor.Common.Models.Message;
using OrderSupervisor.Common.Repositories;
using OrderSupervisorApi.Swagger;
using OrderSupervisorApi.Swagger.SchemaDefinitions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;

namespace OrderSupervisorApi
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
            services.AddControllers(configure =>
            {
                //TODO add unauthorized status code after implementing authorization bearer token logic.
                //configure.Filters.Add(new ProducesResponseTypeAttribute(401));
                //TODO to add validations to route params and return badrequest response.
                //configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), 400));
                configure.Filters.Add(new ProducesResponseTypeAttribute(500));
                configure.Filters.Add(new ProducesResponseTypeAttribute(404));
            });
            services.AddApiVersioning();
            services.AddOptions();
            //Application Insights is added for logging and monitoring purpose
            services.AddApplicationInsightsTelemetry();

            //TODO healthy status check for queue.
            services.AddHealthChecks()
                    .AddApplicationInsightsPublisher();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderSupervisor API", Version = "v1" });

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var versions = apiDesc.CustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v}" == docName);
                });

                c.OperationFilter<RemoveVersionParameters>();
                c.DocumentFilter<SetVersionInPaths>();

                c.MapObject();
                c.DescribeAllParametersInCamelCase();

                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "OrderSupervisorApi.xml");
                c.IncludeXmlComments(xmlPath);
            });

            services.AddSwaggerGenNewtonsoftSupport();

            //Create queue if not already exists.
            var storageAccount = Configuration.GetSection("StorageAccount").Get<StorageAccount>();
            var queueClient = new QueueClient(storageAccount.ConnectionString, storageAccount.QueueName);

            if (null != queueClient.CreateIfNotExistsAsync())
            {
                Console.WriteLine("The queue was created.");
            }

            services.AddSingleton<IQueueClientFactory, QueueClientFactory>();
            services.AddSingleton<IQueueOperations<Order>, QueueOperations<Order>>();
            services.AddSingleton<ICloudTableClient, CloudTableClient>();
            services.AddSingleton<IOrderRepository, OrderRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderSupervisorApi V1");
            });

            //TODO this is used to add authorization middleware.
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseHealthChecks("/healthcheck");//health status of Api
        }
    }
}
