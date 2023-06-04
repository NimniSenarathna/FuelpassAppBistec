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
        private readonly Container _fuelTransactionsContainer;

        public FuelIssueApi(CosmosClient cosmosClient, Container fuelTransactionsContainer)
        {
            _cosmosClient = cosmosClient;
            _fuelTransactionsContainer = fuelTransactionsContainer;
        }

        [FunctionName("CheckVehicleRegistration")]
        public async Task<IActionResult> CheckVehicleRegistration(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vehiclenumber/{vehicleNumberPlate}")]
        HttpRequest req, ILogger log, string vehicleNumberPlate)
        {
            log.LogInformation($"Checking vehicle registration for fuel issue. Vehicle number plate: {vehicleNumberPlate}");

            try
            {
                // Check if the vehicle number plate exists in the 'FuelTransaction' container
                var vehicleQuery = new QueryDefinition("SELECT c.VehicleType, c.TotalWeeklyQuota, c.RemainingQuota FROM c WHERE c.VehicleNumber = @vehicleNumberPlate")
                    .WithParameter("@vehicleNumberPlate", vehicleNumberPlate);

                var queryIterator = _fuelTransactionsContainer.GetItemQueryIterator<FuelTransaction>(vehicleQuery);
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


        //--- Function to reduce the fuel quota ---

        [FunctionName("ReduceFuelQuota")]
        public async Task<IActionResult> ReduceFuelQuota(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "reducefuel/{id}/{fuelAmount}")] 
             HttpRequest req, ILogger log, string Id, int fuelAmount)
        {
            log.LogInformation("Reducing fuel quota.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<ReduceFuelRequest>(requestBody);

                // Retrieve the fuel transaction based on the vehicle number plate
                var fuelTransactionQuery = new QueryDefinition("SELECT * FROM c WHERE c.VehicleNumber = @vehicleNumberPlate")
                    .WithParameter("@vehicleNumberPlate", data.VehicleNumber);

                var queryIterator = _fuelTransactionsContainer.GetItemQueryIterator<FuelTransaction>(fuelTransactionQuery);
                var fuelTransactions = await queryIterator.ReadNextAsync();

                if (fuelTransactions.Count == 0)
                {
                    // Vehicle number is not registered
                    return new NotFoundResult();

                }

                var fuelTransaction = fuelTransactions.First();

                if (fuelTransaction.FuelQuota == null)
                {
                    // Fuel quota is not set for the vehicle
                    return new BadRequestObjectResult("Fuel quota is not set for the vehicle.");
                }

                if (data.fuelAmount > fuelTransaction.FuelQuota)
                {
                    // Fuel amount exceeds the remaining fuel quota
                    return new BadRequestObjectResult("Fuel amount exceeds the remaining fuel quota.");
                }

                // Reduce the fuel quota and update the fuel transaction
                fuelTransaction.FuelQuota -= data.fuelAmount;

                var response = await _fuelTransactionsContainer.ReplaceItemAsync(fuelTransaction, fuelTransaction.Id);

                return new OkObjectResult(response.Resource);
            }

            catch (CosmosException e)
            {
                log.LogError($"Error occurred while reducing fuel quota: {e.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
