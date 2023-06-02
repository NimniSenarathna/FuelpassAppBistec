using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelpassApp.Models
{
    public class IssueFuel
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("vehicleNumberPlate")]
        public string VehicleNumberPlate { get; set; }

        [JsonProperty("fuelAmount")]
        public double FuelAmount { get; set; }
    }
}
