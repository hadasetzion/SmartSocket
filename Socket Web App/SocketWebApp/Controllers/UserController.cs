using Microsoft.AspNetCore.Mvc;
using SocketWebApp.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Session;


namespace SocketWebApp.Controllers
{

    public class UserController : Controller
    {
        private readonly ICosmosDbUserService _cosmosDbService;

        public UserController(ICosmosDbUserService cosmosDbUserService)
        {
            _cosmosDbService = cosmosDbUserService;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult LoginPage()
        {
            return View("Login");
        }

        public async Task<IActionResult> Login(Credentials credentials)
        {
            var user = await _cosmosDbService.GetUserAsync(credentials.Id);
            if (user == null)
            {
                return View("failedLogin_noUser");
            }
            else
            {
                if (user.UserName == credentials.UserName && user.Password == credentials.Password) //validate info
                {
                    //set user session here
                    HttpContext.Session.SetString("userInfo", JsonConvert.SerializeObject(user));

                    return View("AfterSignIn");
                }
                else
                {
                    return View("failedLogin_wrongCredentials");
                }
            }
        }

        public IActionResult LogOut()
        {
            if (HttpContext.Session.GetString("userInfo") != null)
            {
                HttpContext.Session.Remove("userInfo");
            }
            return RedirectToAction("Main", "Home");
        }

        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        [ActionName("EditPersonal")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPersonalAsync([Bind("Id,FirstName,LastName,Email,Mobile,CardNumber,Expiry,CVV,UserName,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                await _cosmosDbService.UpdateUserAsync(user.Id, user);
                return RedirectToAction("Settings", "User");
            }

            return View(user);
        }

        [ActionName("EditPersonal")]
        public async Task<ActionResult> EditPersonalAsync()
        {

            if (HttpContext.Session.GetString("userInfo") != null)
            {
                //get user session object
                var userInfo = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("userInfo"));

                User user = await _cosmosDbService.GetUserAsync(userInfo.Id);

                if (user.Id == null)
                {
                    return BadRequest();
                }

                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            else
            {
                return View("SignInRequest");
            }

        }

        [HttpPost]
        [ActionName("EditPayment")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPaymentAsync([Bind("Id,FirstName,LastName,Email,Mobile,CardNumber,Expiry,CVV,UserName,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                await _cosmosDbService.UpdateUserAsync(user.Id, user);
                return RedirectToAction("Settings","User");
            }

            return View(user);
        }

        [ActionName("EditPayment")]
        public async Task<ActionResult> EditPaymentAsync()
        {
            if (HttpContext.Session.GetString("userInfo") != null)
            {
                var userInfo = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("userInfo"));

                User user = await _cosmosDbService.GetUserAsync(userInfo.Id);

                if (user.Id == null)
                {
                    return BadRequest();
                }

                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            else
            {
                return View("SignInRequest");
            }
        }


        public IActionResult AfterSignUp()
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
        public async Task<ActionResult> CreateAsync([Bind("Id,FirstName,LastName,Email,Mobile,CardNumber,Expiry,CVV,UserName,Password")] User user)
        {
            //set user session here
            HttpContext.Session.SetString("userInfo", JsonConvert.SerializeObject(user));

            if (ModelState.IsValid)
            {
                await _cosmosDbService.AddUserAsync(user);
                return RedirectToAction("AfterSignUp");
            }

            return View(user);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("Id,FirstName,LastName,Email,Mobile,CardNumber,Expiry,CVV,UserName,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                await _cosmosDbService.UpdateUserAsync(user.Id, user);
                return RedirectToAction("Index");
            }

            return View(user);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            User user = await _cosmosDbService.GetUserAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            User user = await _cosmosDbService.GetUserAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id")] string id)
        {
            await _cosmosDbService.DeleteUserAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            return View(await _cosmosDbService.GetUserAsync(id));
        }
    }
}