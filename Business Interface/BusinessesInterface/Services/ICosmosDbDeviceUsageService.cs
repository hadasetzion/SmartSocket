namespace BusinessesInterface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BusinessesInterface.Models;

    public interface ICosmosDbDeviceUsageService
    {
        Task<Address> GetUsageHistoryAsync(string query, string show = null , string socketNumber = null);
        Task<Address> GetUsageHistoryByTimeAsync(string queryString, TimeSearch searchDate, string searchFor);
        Task<Address> GetLoginDetails(string querystring);
        Task<Address> GetTablesMap(string queryString);
        Task<Address> GetDevices(string queryString);
    }
}