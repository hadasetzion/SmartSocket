using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocketWebApp.Models;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocketWebApp
{
    public class CosmosDbUserService : ICosmosDbUserService
    {
        private Container _container;

        public CosmosDbUserService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddUserAsync(Models.User user)
        {
            await this._container.CreateItemAsync<Models.User>(user, new PartitionKey(user.Id));
        }

        public async Task DeleteUserAsync(string id)
        {
            await this._container.DeleteItemAsync<Models.User>(id, new PartitionKey(id));
        }

        public async Task<Models.User> GetUserAsync(string id)
        {
            try
            {
                ItemResponse<Models.User> response = await this._container.ReadItemAsync<Models.User>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

        }

        public async Task<IEnumerable<Models.User>> GetUsersAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Models.User>(new QueryDefinition(queryString));
            List<Models.User> results = new List<Models.User>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateUserAsync(string id, Models.User user)
        {
            await this._container.UpsertItemAsync<Models.User>(user, new PartitionKey(id));
        }
    }
}

