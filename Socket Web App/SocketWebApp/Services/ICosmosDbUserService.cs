using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SocketWebApp.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocketWebApp
{
    public interface ICosmosDbUserService
    {
        Task<IEnumerable<User>> GetUsersAsync(string query);
        Task<User> GetUserAsync(string id);
        Task AddUserAsync(User item);
        Task UpdateUserAsync(string id, User user);
        Task DeleteUserAsync(string id);
    }
}

