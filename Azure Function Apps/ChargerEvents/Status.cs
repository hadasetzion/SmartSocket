using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChargerEvents
{
    public static class Status
    {
        [FunctionName("Status")]
        public static async Task Run(
            [EventHubTrigger("chargerevents", Connection = "Connection_String")] EventData[] events,
            [CosmosDB(
            databaseName: "chargerDB",
            collectionName: "addresses",
            Id = "0",
            PartitionKey = "0",
            ConnectionStringSetting = "CosmosDBConnectionString")] Address address0,
            [CosmosDB(
            databaseName: "chargerDB",
            collectionName: "addresses",
            Id = "1",
            PartitionKey = "1",
            ConnectionStringSetting = "CosmosDBConnectionString")] Address address1,
            [CosmosDB(
            databaseName: "chargerDB",
            collectionName: "addresses",
            Id = "2",
            PartitionKey = "2",
            ConnectionStringSetting = "CosmosDBConnectionString")] Address address2,
            [CosmosDB(
            databaseName: "chargerDB",
            collectionName: "addresses",
            Id = "3",
            PartitionKey = "3",
            ConnectionStringSetting = "CosmosDBConnectionString")] Address address3,
            ILogger log)
        {
            string deviceID = "";
            string status = "";
            var exceptions = new List<Exception>();
          
            log.LogCritical("hi");
            foreach (EventData eventData in events)
            {
                try
                {
                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {eventData.EventBody}");
                    //eventString = eventData.EventBody.ToString();
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(eventData.EventBody.ToString());
                    deviceID = dict["device_id"];
                    status = dict["status"];

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
                
                foreach (Device device in address0.devices)
                {
                    if (!device.deviceID.Equals(deviceID))
                    {
                        continue;
                    }

                    device.status = status;
                    device.last_seen = (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
                }
                

                foreach (Device device in address1.devices)
                {
                    if (!device.deviceID.Equals(deviceID))
                    {
                        continue;
                    }

                    device.status = status;
                    device.last_seen = (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
                }

                foreach (Device device in address2.devices)
                {
                    if (!device.deviceID.Equals(deviceID))
                    {
                        continue;
                    }

                    device.status = status;
                    device.last_seen = (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
                }

                foreach (Device device in address3.devices)
                {
                    if (!device.deviceID.Equals(deviceID))
                    {
                        continue;
                    }

                    device.status = status;
                    device.last_seen = (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
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
