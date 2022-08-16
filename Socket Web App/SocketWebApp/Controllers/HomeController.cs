using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocketWebApp.Models;

namespace SocketWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(string parameterInfo)
    {
        //set parameter session here
        HttpContext.Session.SetString("parameterInfo", JsonConvert.SerializeObject(parameterInfo));
        return View();
    }

    public IActionResult Main()
    {
        if (HttpContext.Session.GetString("userInfo") == null && HttpContext.Session.GetString("guestInfo") == null)
        {
            return RedirectToAction("MainOut", "Home");
        }
        else
        {
            return View();

        }
    }

    public IActionResult MainOut()
    {
        return View();
    }


    public IActionResult Terms()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

