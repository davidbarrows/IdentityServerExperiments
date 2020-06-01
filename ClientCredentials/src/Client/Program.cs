using System;
using System.Threading.Tasks;
using Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Client
{
    public class Program
    {
        private static async Task<int> Main()
        {
            // dB notes 21/05/2020 - 29/05/2020
            // this started life as the original sample file from the IdentityServer4 Quickstart.
            // This has been converted to integration tests in my example;
            // but now I've also refactored it to make it more testable;
            // created a couple of services and an 'orchestrator' class to call them

            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Starting client application");

            var hostBuilder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient("identityServer", c =>
                    {
                        // TODO:  configure for IdentityServer -
                        // the Quickstart specifies no BaseAddress, etc
                    });
                    
                    services.AddHttpClient("api", c =>
                    {
                        // TODO:  configure for the API - 
                        // same as above
                    });

                    services.AddTransient<IApiClientService, ApiClientService>();
                    services.AddTransient<ISecurityService, SecurityService>();
                    services.AddLogging();
                }).UseConsoleLifetime();

            var host = hostBuilder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;

                try
                {
                    var apiClientService = serviceProvider.GetService<IApiClientService>();
                    var securityService = serviceProvider.GetService<ISecurityService>();
                    var orchestrator = new ClientOrchestrator(securityService, apiClientService, logger);

                    await orchestrator.Execute();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred.");
                }
            }





            




            
            return 0;

        }
    }
}