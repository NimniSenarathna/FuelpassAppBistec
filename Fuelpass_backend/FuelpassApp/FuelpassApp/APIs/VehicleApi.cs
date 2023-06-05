using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using FuelpassApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FuelpassApp.APIs
{
    public class VehicleApi
    {

        private readonly CosmosClient _cosmosClient;
        private Container documentContainer;
        private readonly Container _fuelTransactionContainer;
        private readonly Container _fuelQuotaContainer;

        public VehicleApi(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            documentContainer = _cosmosClient.GetContainer("fuelpass-application", "item");
            _fuelTransactionContainer = _cosmosClient.GetContainer("fuelpass-application", "FuelTransaction");
            _fuelQuotaContainer = _cosmosClient.GetContainer("fuelpass-application", "VehicleFuelQuota");
        }

        [FunctionName("Vehicle")]


        public async Task<IActionResult> CreateVehicle(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Vehicle")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("Creating Register page Items");
            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CreateVehicle>(requestData);

            var vehicle = new Vehicle
            {
                firstName = data.firstName,
                lastName = data.lastName,
                email = data.email,
                vehicleType = data.vehicleType,
                vehicleChassisNumber = data.vehicleChassisNumber,
                fuelType = data.fuelType,
                vehicleNumberPlate = data.vehicleNumberPlate
            };

            await documentContainer.CreateItemAsync(vehicle, new Microsoft.Azure.Cosmos.PartitionKey(vehicle.Id));

            // Create the FuelTransaction container
            var fuelTransaction = new FuelTransaction
            {
                Id = Guid.NewGuid().ToString(),
                VehicleNumber = vehicle.vehicleNumberPlate,
                VehicleType = vehicle.vehicleType
            };

            // Retrieve the assigned fuel quota from the VehicleFuelQuota container based on the vehicle type
            var query = new QueryDefinition("SELECT c.Quota FROM c WHERE c.Type = @vehicleType")
                .WithParameter("@vehicleType", vehicle.vehicleType);
            var iterator = _fuelQuotaContainer.GetItemQueryIterator<dynamic>(query);

            var results = await iterator.ReadNextAsync();
            var quota = results.FirstOrDefault()?.Quota;

            if (quota != null)
            {
                fuelTransaction.FuelQuota = quota;
            }

            // Stores the fuel transaction
            await _fuelTransactionContainer.CreateItemAsync(fuelTransaction, new PartitionKey(fuelTransaction.Id));

            return new OkObjectResult(vehicle);
        }

    }
}
