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
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace FuelpassApp.APIs
{
    public class FuelIssueApi
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _fuelTransactionsContainer;
        private readonly Container _vehicleFuelQuotaContainer;
      

        public FuelIssueApi(CosmosClient cosmosClient, Container fuelTransactionsContainer, Container vehicleFuelQuotaContainer)
        {
            _cosmosClient = cosmosClient;
            _fuelTransactionsContainer = fuelTransactionsContainer;
            _vehicleFuelQuotaContainer = vehicleFuelQuotaContainer;
        }

        static List<FuelTransaction> fuelTransactionList = new();

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


        [FunctionName("GetRemainingQuota")]
        public async Task<IActionResult> GetRemainingQuota(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FuelApi/GetRemainingQuota/{vehicleNumberPlate}")]
          HttpRequest req, ILogger log, string vehicleNumberPlate)
        {
            log.LogInformation($"Getting remaining quota for vehicle: {vehicleNumberPlate}");

            try
            {
                // Check if the vehicle number plate exists in the 'FuelTransaction' container
                var vehicleQuery = new QueryDefinition("SELECT * FROM c WHERE c.VehicleNumber = @vehicleNumberPlate")
                    .WithParameter("@vehicleNumberPlate", vehicleNumberPlate);

                var queryIterator = _fuelTransactionsContainer.GetItemQueryIterator<dynamic>(vehicleQuery);
                var fuelTransactions = await queryIterator.ReadNextAsync();

                //debug
                log.LogInformation(JsonConvert.SerializeObject(fuelTransactions));

                if (fuelTransactions.Count > 0)
                {
                    // Vehicle number is registered
                    var fuelTransaction = fuelTransactions.First();
                    double remainingQuota = fuelTransaction.FuelQuota;

                    return new OkObjectResult(remainingQuota);
                }
                else
                {
                    // Vehicle number is not registered
                    return new NotFoundResult();
                }
            }
            catch (CosmosException e)
            {
                log.LogError($"Error occurred while getting remaining quota: {e.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        [FunctionName("ReduceFuelQuotaFunction")]
        public async Task<IActionResult> ReduceFuelQuotaFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "ReduceFuelQuota/{vehicleNumberPlate}")]
            HttpRequest req, ILogger log, string vehicleNumberPlate)
        {
            log.LogInformation("Reducing fuel quota.");

            try
            {
                // Retrieve the fuel transaction based on the vehicle number plate
                var fuelTransactionQuery = new QueryDefinition("SELECT * FROM c WHERE c.VehicleNumber = @vehicleNumberPlate")
                    .WithParameter("@vehicleNumberPlate", vehicleNumberPlate);

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
                    return new BadRequestObjectResult("Fuel quota is not set for the vehicle.");
                }

                // Read the payload from the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                double reductionAmount = data?.reductionAmount;

                if (reductionAmount == null || reductionAmount <= 0)
                {
                    return new BadRequestObjectResult("Invalid reduction amount.");
                }

                if (fuelTransaction.FuelQuota < reductionAmount)
                {
                    return new BadRequestObjectResult("Reduction amount exceeds the available fuel quota.");
                }

                // Reduce the fuel quota by the consuming amount
                fuelTransaction.FuelQuota -= reductionAmount;

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

