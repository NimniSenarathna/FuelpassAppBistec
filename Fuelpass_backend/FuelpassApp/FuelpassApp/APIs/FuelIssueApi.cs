using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FuelpassApp.Models;
using System.Linq;

namespace FuelpassApp.APIs
{
    public class FuelIssueApi
    {
        private readonly CosmosClient _cosmosClient;
        private Container _registeredVehiclesContainer;

        public FuelIssueApi()
        {
            string cosmosDbEndpointUrl = "https://localhost:8081";
            string cosmosDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            _cosmosClient = new CosmosClient(cosmosDbEndpointUrl, cosmosDbKey);
            _registeredVehiclesContainer = _cosmosClient.GetContainer("fuelpass-application", "item");
        }

        [FunctionName("CheckVehicleRegistration")]
        public async Task<IActionResult> CheckVehicleRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vehiclenumber/{id}/{vehicleNumberPlate}")]
            HttpRequest req, ILogger log, string id, string vehicleNumberPlate)
        {
            log.LogInformation($"Checking vehicle registration for fuel issue. Vehicle number plate: {vehicleNumberPlate}");

            try
            {
                // Check if the vehicle number plate exists in the 'item' container
                var vehicleQuery = new QueryDefinition("SELECT * FROM c WHERE c.vehicleNumberPlate = @vehicleNumberPlate")
                    .WithParameter("@vehicleNumberPlate", vehicleNumberPlate);

                var queryIterator = _registeredVehiclesContainer.GetItemQueryIterator<Vehicle>(vehicleQuery);
                var registeredVehicles = await queryIterator.ReadNextAsync();

                if (registeredVehicles.Count > 0)
                {
                    // Vehicle number is registered
                    var registeredVehicle = registeredVehicles.First();
                    return new OkObjectResult(registeredVehicle);
                }
                else
                {
                    // Vehicle number is not registered
                    return new NotFoundResult();
                }

            }
            catch (CosmosException e)
            {
                log.LogError($"Error occurred while checking vehicle registration: {e.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
