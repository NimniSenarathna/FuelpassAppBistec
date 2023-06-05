using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FuelpassApp;
using System;
using FuelpassApp.APIs;


[assembly: FunctionsStartup(typeof(Startup))]

namespace FuelpassApp
{
    public class Startup : FunctionsStartup
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", true)
            .AddEnvironmentVariables()
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(s =>
            {
                var connectionString = Configuration["CosmosDBConnection"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDBConnection in the appSettings.json file or your Azure Functions Settings.");
                }

                var cosmosClient = new CosmosClientBuilder(connectionString)
                    .Build();

                return cosmosClient;
            });

            // Register FuelIssueApi
            builder.Services.AddSingleton<FuelIssueApi>(); 

            builder.Services.AddSingleton(s =>
            {
                var cosmosClient = s.GetRequiredService<CosmosClient>();
                var fuelTransactionsContainer = cosmosClient.GetContainer("fuelpass-application", "FuelTransaction");
                var vehicleFuelQuotaContainer = cosmosClient.GetContainer("fuelpass-application", "VehicleFuelQuota");

                // Register the containers
                return fuelTransactionsContainer;
            });
        }
    }
}
