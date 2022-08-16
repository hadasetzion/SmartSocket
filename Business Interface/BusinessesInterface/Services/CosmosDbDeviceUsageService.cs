namespace BusinessesInterface
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using BusinessesInterface.Models;
    using System.Globalization;

    public class CosmosDbDeviceUsageService : ICosmosDbDeviceUsageService
    {
        private Container _container;

        public CosmosDbDeviceUsageService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<Address> GetUsageHistoryAsync(string queryString, string show = null, string socketNumber = null) //Perform search in DB for the general case and for the specific socket case
        {
            if(socketNumber != null)
            {
                queryString = queryString + $"AND device.id = '{socketNumber}'";
            }
        
            var query = this._container.GetItemQueryIterator<Address>(new QueryDefinition(queryString));

            Address result = null;
            int buisnessOverAllUsage = 0;
         
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                if(response.Count == 0)
                {
                    break; //returns result=null
                }
                var buisness = response.ToList()[0];
                DateTime dateTime;
                
                foreach (var device in buisness.Devices)
                {
                    int socketOverAllUsage = 0;
                    if (socketNumber != null && !device.Id.Equals(socketNumber))
                    {
                        continue;
                    }
                    var tsList = new List<DateTime>();

                    if (show.Equals("full"))
                    {
                        foreach (var timeStamp in device.History)
                        {
                            dateTime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).DateTime.AddHours(3);
                            tsList.Add(dateTime);
                        }
                    }
                    else
                    {
                        if(device.History.Length > 0)
                        {
                            var timeStamp = device.History[device.History.Length - 1];
                            dateTime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).DateTime.AddHours(3);
                            tsList.Add(dateTime);

                        }
                      
                    }

                    socketOverAllUsage = device.History.Length;
                    buisnessOverAllUsage += socketOverAllUsage;
                    device.OverAllUsage = socketOverAllUsage;
                    device.HistoryDateTime = tsList.ToArray();
                }
                buisness.OverAllUsage = buisnessOverAllUsage;
                result = buisness;
            }
            return result;
        }

        public async Task<Address> GetUsageHistoryByTimeAsync(string queryString, TimeSearch searchDate, string searchFor)
        {
            var query = this._container.GetItemQueryIterator<Address>(new QueryDefinition(queryString)); //first try to fetch all results for this buisness

            Address results = null;
            int buisnessOverAllUsage = 0;
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                if(response.Count == 0)
                {
                    break; //returns result=null
                }
                var buisness = response.ToList()[0];
                DateTime dateTime;
                int notFound = 0;

                foreach (var device in buisness.Devices)
                {
                    var tsList = new List<DateTime>();
                    int socketOverAllUsage = 0;
                    foreach (var timeStamp in device.History)
                    {
                        dateTime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).DateTime.AddHours(3);
                        var dateTimeDay = dateTime.Date;
                        var dateSearchDate = searchDate.TimeToSearch.Date;
                        if (searchFor.Equals("specific") && dateSearchDate == dateTimeDay)
                        {
                            tsList.Add(dateTime);
                        }else if(searchFor.Equals("since") && dateSearchDate <= dateTimeDay)
                        {
                            tsList.Add(dateTime);
                        }
                        else
                        {
                            continue; //this dateTime does not match the search, continue
                        }
                    }
                    if(tsList.Count == 0) //could not find the search within this device
                    {
                        notFound ++;
                    }
                    socketOverAllUsage = tsList.Count;
                    buisnessOverAllUsage += socketOverAllUsage;
                    device.HistoryDateTime = tsList.ToArray();
                    device.OverAllUsage = socketOverAllUsage;
                }
                buisness.OverAllUsage = buisnessOverAllUsage;
                if (!(notFound == buisness.Devices.Length))
                {
                    results = buisness;
                }
            }
            return results; //will return null if notFound == buisness.Devices.Length
        }

        public async Task<Address> GetDevices(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Address>(new QueryDefinition(queryString));
            Address result = null;

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                if (response.Count == 0)
                {
                    break; //returns result=null
                }
                var buisness = response.ToList()[0];
                foreach(var device in buisness.Devices)
                {
                    var lastSeen = DateTimeOffset.FromUnixTimeSeconds(device.LastSeen).DateTime;
                    var now = System.DateTime.UtcNow;
                    var tenGap = new System.TimeSpan(0, 10, 0);
               
                    if (now.Subtract(lastSeen) <= tenGap)
                    {
                        device.HealthCheck = "OK";
                    }
                    else
                    {
                        device.HealthCheck = "Not Stable";
                    }
                }
                result = buisness;
            }
            return result;
        }

        public async Task<Address> GetLoginDetails(string querystring)
        {
            var query = this._container.GetItemQueryIterator<Address>(new QueryDefinition(querystring));
            Address? result = null;
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                if(response.Count == 0)
                {
                    break;
                }
                var item = response.ToList()[0];
                result = item;
            }
            return result;
        }

        public async Task<Address> GetTablesMap(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Address>(new QueryDefinition(queryString));

            Address result = null;

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                if (response.Count == 0)
                {
                    break; //returns result=null
                }
                result = response.ToList()[0];
            }
            return result;
        }

    }
}