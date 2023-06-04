using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelpassApp.Models
{
    public class FuelTransaction
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("VehicleNumber")]
        public string VehicleNumber { get; set; }

        [JsonProperty("VehicleType")]
        public string VehicleType { get; set; }

        [JsonProperty("FuelQuota")]
        public int? FuelQuota { get; set; }
    }

    public class ReduceFuelRequest
    {
        public string VehicleNumber { get; set; }
        public int fuelAmount { get; set; }
    }
}
