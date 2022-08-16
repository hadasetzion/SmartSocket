using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocketWebApp.Models;

namespace SocketWebApp.Controllers
{
    public class GuestController : Controller
    {
        private readonly ICosmosDbGuestService _cosmosDbService;

        public GuestController(ICosmosDbGuestService cosmosDbUserService)
        {
            _cosmosDbService = cosmosDbUserService;
        }

        public IActionResult AfterGuestEntry()
        {
            return View();
        }

        [ActionName("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Id,FirstName,LastName,Email,Mobile,CardNumber,Expiry,CVV")] Guest guest)
        {
            if (ModelState.IsValid)
            {
                //set guest session here
                HttpContext.Session.SetString("guestInfo", JsonConvert.SerializeObject(guest));

                TimeSpan t = DateTime.Now - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;

                //anonymize guests
                guest.Id += secondsSinceEpoch.ToString();
                guest.FirstName = "***";
                guest.LastName = "***";
                guest.Email = "***";
                guest.Mobile = "***";

                await _cosmosDbService.AddGuestAsync(guest);
                return RedirectToAction("AfterGuestEntry"); 
            }

            return View(guest);
        }

    }
}

