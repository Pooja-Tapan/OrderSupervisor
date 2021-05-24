using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderSupervisor.Common.AzureQueue;
using OrderSupervisor.Common.Repositories;
using System;
using System.IO;
using System.Threading;

namespace Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            var cts = new CancellationTokenSource();
            try
            {
                // calls the Run method in App for processing
                serviceProvider.GetService<IApp>().Run(cts.Token);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //To remove all the objects created.
                if (serviceProvider is IDisposable)
                {
                    ((IDisposable)serviceProvider).Dispose();
                }
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            //Register your services here.
            var config = LoadConfiguration();
            services.AddSingleton(config);

            services.AddSingleton<IRetryIntervalGenerator, ExponentialRetryIntervalGenerator>();            
            // required to run the application
            services.AddTransient<IApp, App>();

            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}
