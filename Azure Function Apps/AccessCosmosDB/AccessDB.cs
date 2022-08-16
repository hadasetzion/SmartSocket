using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.EventGrid.Models;

namespace AccessCosmosDB
{
    public static class AcccessDB
    {
        static ServiceClient serviceClient;
        static string iotHubconnectionString = "HostName=chargerHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=/UdsGI9r0ZzTuXZAXFP6jvHMvZ5b7AcTaYhwKhFziJU=";

        private async static Task turnOnLED(String targetDevice)
        {
            var commandMessage = new
             Message(Encoding.ASCII.GetBytes("on"));
            await serviceClient.SendAsync(targetDevice, commandMessage);
        }

        private async static Task turnOffLED(String targetDevice)
        {
            var commandMessage = new
             Message(Encoding.ASCII.GetBytes("off"));
            await serviceClient.SendAsync(targetDevice, commandMessage);
        }



        [FunctionName("ConnectDevice")]
        public static void Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ConnectDevice/{addr}/{dev}")] HttpRequestMessage req,
            [CosmosDB(
                databaseName: "chargerDB",
                collectionName: "addresses",
                ConnectionStringSetting = "CosmosDBConnectionString",
                SqlQuery ="SELECT device.deviceID FROM addresses address JOIN device IN address.devices WHERE address.id={addr} AND device.id={dev}")] IEnumerable<Device> devices,
            [CosmosDB(
                databaseName: "chargerDB",
                collectionName: "addresses",
                Id = "{addr}",
                PartitionKey = "{addr}",
                ConnectionStringSetting = "CosmosDBConnectionString")] Address address,
            [CosmosDB(
                databaseName: "chargerDB",
                collectionName: "addresses",
                Id = "{addr}",
                PartitionKey = "{addr}",
                ConnectionStringSetting = "CosmosDBConnectionString")] out Address update,
            string dev,
            ILogger log)
        
        {
            foreach (Device device in devices)
            {
                log.LogInformation(device.deviceID);

                serviceClient = ServiceClient.CreateFromConnectionString(iotHubconnectionString);
                string targetDevice = device.deviceID;
                turnOnLED(targetDevice).Wait();
                log.LogInformation("Light is on");
            }

            foreach (Device device in address.devices)
            {
                if (!device.id.Equals(dev))
                {
                    continue;
                }

                device.history.Add(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());
            }

            update = address;
        }

        [FunctionName("DeviceOff")]
        public static void RunOff(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeviceOff/{addr}/{dev}")] HttpRequestMessage req,
            [CosmosDB(
                databaseName: "chargerDB",
                collectionName: "addresses",
                ConnectionStringSetting = "CosmosDBConnectionString",
                SqlQuery ="SELECT device.deviceID FROM addresses address JOIN device IN address.devices WHERE address.id={addr} AND device.id={dev}")] IEnumerable<Device> devices,
            ILogger log)

        {
            foreach (Device device in devices)
            {
                log.LogInformation(device.deviceID);

                serviceClient = ServiceClient.CreateFromConnectionString(iotHubconnectionString);
                string targetDevice = device.deviceID;
                turnOffLED(targetDevice).Wait();
                log.LogInformation("Light is off");
            }
        }


        [FunctionName("AllDevicesOff")]
        public static void AllOff(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AllDevicesOff/{addr}")] HttpRequestMessage req,
           [CosmosDB(
                databaseName: "chargerDB",
                collectionName: "addresses",
                ConnectionStringSetting = "CosmosDBConnectionString",
                SqlQuery ="SELECT device.deviceID FROM addresses address JOIN device IN address.devices WHERE address.id={addr}")] IEnumerable<Device> devices,
           ILogger log)

        {
            foreach (Device device in devices)
            {
                log.LogInformation(device.deviceID);

                serviceClient = ServiceClient.CreateFromConnectionString(iotHubconnectionString);
                string targetDevice = device.deviceID;
                turnOffLED(targetDevice).Wait();
                log.LogInformation("Light is off");
            }
        }


        [FunctionName("ChangeTable")]
        public static void RunTable(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ChangeTable/{addr}/{dev}/{table}")] HttpRequestMessage req,
            [CosmosDB(
                databaseName: "chargerDB",
                collectionName: "addresses",
                Id = "{addr}",
                PartitionKey = "{addr}",
                ConnectionStringSetting = "CosmosDBConnectionString")] Address address,
            [CosmosDB(
                databaseName: "chargerDB",
                collectionName: "addresses",
                Id = "{addr}",
                PartitionKey = "{addr}",
                ConnectionStringSetting = "CosmosDBConnectionString")] out Address update,
            string dev, string table,
            ILogger log)

        {
            foreach (Device device in address.devices)
            {
                if (!device.id.Equals(dev))
                {
                    continue;
                }

                device.table = table;
            }

            update = address;
        }
    }




    public class Address
    {
        public string id { get; set; }
        public Device[] devices { get; set; }
        public string password { get; set; }
    }

    public class Device
    {
        public string id { get; set; }
        public string deviceID { get; set; }
        public dynamic history { get; set; }
        public string status { get; set; }
        public long last_seen { get; set; }
        public string table { get; set; }
    }
}
