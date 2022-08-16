using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocketWebApp.Models;
using System.Net.Http;

namespace SocketWebApp
{
    public class DeviceController : Controller
    {
        private static HttpClient client = new HttpClient();
        public IActionResult Index()
        {
            return View();
        }

        [ActionName("Activation")]
        public async Task<IActionResult> Activation() 
        {
            if (HttpContext.Session.GetString("guestInfo") == null && HttpContext.Session.GetString("userInfo") == null)
            {
                return View("EntryRequest");
            }

            else
            {
                if(HttpContext.Session.GetString("parameterInfo") != null)
                {
                    //get parameter session object
                    var parameterInfo = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("parameterInfo"));

                    //device activation action using parameterInfo as address and device
                    var response = await client.PostAsync($"https://accesscosmosdb20220807160933.azurewebsites.net/api/ConnectDevice/{parameterInfo}", null);
                    return View();
                }
                else
                {
                    return View("ScanAgain");

                }

            }
        }

        [ActionName("De_Activation")]
        public async Task<IActionResult> De_Activation() 
        {

            if (HttpContext.Session.GetString("parameterInfo") != null)
            {
                //get parameter session object
                var parameterInfo = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("parameterInfo"));

                //device de-activation action using parameterInfo as address and device
                var response = await client.PostAsync($"https://accesscosmosdb20220807160933.azurewebsites.net/api/DeviceOff/{parameterInfo}", null);
                return View();
            }
            else
            {
                return View();

            }

        }

    }
}

