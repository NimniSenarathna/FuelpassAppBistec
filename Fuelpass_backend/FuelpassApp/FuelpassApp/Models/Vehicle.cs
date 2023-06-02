using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FuelpassApp.Models
{
    public class Vehicle
    {

        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string vehicleType { get; set; }
        public string vehicleChassisNumber { get; set; }
        public string fuelType { get; set; }
        public string vehicleNumberPlate { get; set; }
        
    }

    public class CreateVehicle
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string vehicleType { get; set; }
        public string vehicleChassisNumber { get; set; }
        public string fuelType { get; set; }
        public string vehicleNumberPlate { get; set; }
    }
}
