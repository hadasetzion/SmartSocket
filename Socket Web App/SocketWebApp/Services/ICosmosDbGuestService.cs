using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SocketWebApp.Models;

namespace SocketWebApp
{
    public interface ICosmosDbGuestService
    {
        Task AddGuestAsync(Guest guest);
    }
}

