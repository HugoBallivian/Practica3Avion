using AvionesDistribuidos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Nager.Country;
using System.Linq;

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
            var countryProvider = new CountryProvider();
            var countries = countryProvider.GetCountries().ToList();

            var countryList = countries.Select(c => new CountryViewModel
            {
                Name = c.CommonName,
                Capital = "Sin informaci�n"
            }).ToList();

            var flights = new List<string>
            {
                "B-101 Ucrania(Kiev) - Bolivia(Cochabamba) 18:55 20/Abr/2025",
                "A-205 Espa�a(Madrid) - Argentina(Buenos Aires) 09:30 15/Abr/2025"
            };

            ViewBag.Countries = countryList;
            ViewBag.Flights = flights;


            char fcRowStart = 'A';
            char fcRowEnd = 'F'; 
            int fcColStart = 1;
            int fcColEnd = 3;   

            var firstClassRows = new List<SeatRow>();
            for (char row = fcRowStart; row <= fcRowEnd; row++)
            {
                var seats = Enumerable.Range(fcColStart, fcColEnd - fcColStart + 1)
                    .Select(col => new Seat { SeatId = $"{row}{col.ToString("D2")}" })
                    .ToList();
                firstClassRows.Add(new SeatRow { RowLabel = row.ToString(), Seats = seats });
            }

            char econRowStart = 'A';
            char econRowEnd = 'F'; 
            int econColStart = 4;
            int econColEnd = 29;  

            var economyRows = new List<SeatRow>();
            for (char row = econRowStart; row <= econRowEnd; row++)
            {
                var seats = Enumerable.Range(econColStart, econColEnd - econColStart + 1)
                    .Select(col => new Seat { SeatId = $"{row}{col.ToString("D2")}" })
                    .ToList();
                economyRows.Add(new SeatRow { RowLabel = row.ToString(), Seats = seats });
            }

            var seatConfig = new List<SeatConfiguration>
            {
                new SeatConfiguration
                {
                    SectionName = "Primera Clase",
                    Rows = firstClassRows
                },
                new SeatConfiguration
                {
                    SectionName = "Econ�mica",
                    Rows = economyRows
                }
            };

            ViewBag.SeatConfig = seatConfig;

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
