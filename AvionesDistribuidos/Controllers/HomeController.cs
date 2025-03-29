using AvionesDistribuidos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AvionesDistribuidos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var countryList = new List<CountryViewModel>
            {
                new CountryViewModel { Name = "Colombia", Capital = "Bogotá" },
                new CountryViewModel { Name = "Bolivia", Capital = "Cochabamba" },
                new CountryViewModel { Name = "Ucrania", Capital = "Kiev" },
                new CountryViewModel { Name = "España", Capital = "Madrid" },
                new CountryViewModel { Name = "Argentina", Capital = "Buenos Aires" }
            };

                    var flights = new List<string>
            {
                "B-101 Ucrania(Kiev) - Bolivia(Cochabamba) 18:55 20/Abr/2025",
                "A-205 España(Madrid) - Argentina(Buenos Aires) 09:30 15/Abr/2025"
            };

            ViewBag.Countries = countryList;
            ViewBag.Flights = flights;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
