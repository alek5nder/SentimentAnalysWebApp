using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjektTI.Models;
using System.Diagnostics;

namespace ProjektTI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //do instruckji przchodzimy
        public IActionResult Instructions()
        {
            return View();
        }


        [Authorize(Roles = "Premium")] //autoryzacja uzytk. premium
        public IActionResult Index()
        {
            var displayHelp = true;
            return View(displayHelp);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Public()
        {
            return Content("This page is public.");
        }


        public IActionResult Instructions()
        {
            return View("Instructions");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}