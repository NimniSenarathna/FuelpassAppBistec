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

namespace FuelpassApp.APIs
{
    public class VehicleApi
    {

        private readonly CosmosClient _cosmosClient;
        private Container documentContainer;

        public VehicleApi(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            documentContainer = _cosmosClient.GetContainer("fuelpass-application", "item");
        }

        [FunctionName("Vehicle")]


        public async Task<IActionResult> CreateVehicle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Vehicle")] HttpRequest req,

            ILogger log)
        {
            log.LogInformation("Creating Register page Items");
            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CreateVehicle>(requestData);

            var item = new Vehicle
            {
                firstName = data.firstName,
                lastName = data.lastName,
                email = data.email,
                vehicleType = data.vehicleType,
                vehicleChassisNumber = data.vehicleChassisNumber,
                fuelType = data.fuelType,
                vehicleNumberPlate = data.vehicleNumberPlate
            };

            await documentContainer.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(item.Id));

            return new OkObjectResult(item);
        }
    }
}
