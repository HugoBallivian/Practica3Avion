using AvionesDistribuidos.Data;
using AvionesDistribuidos.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Globalization; // Se requiere para el formateo de fechas

namespace AvionesDistribuidos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "country-by-continent.json");
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            var countryJsonList = JsonConvert.DeserializeObject<List<CountryJson>>(jsonData);

            var countryList = countryJsonList.Select(c => new CountryViewModel
            {
                Name = c.country,
                Capital = c.continent
            }).ToList();
            ViewBag.Countries = countryList;

            var flights = new List<string>
            {
                "Ingrese datos corresponientes"
            };
            ViewBag.Flights = flights;

            var destinosList = _context.Destinos.ToList();
            ViewBag.Destinos = destinosList;

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
                    .Select(col =>
                    {
                        var seat = new Seat { SeatId = $"{row}{col.ToString("D2")}" };
                        if ((row == 'A' || row == 'F') && seat.SeatId.EndsWith("10"))
                        {
                            seat.State = "empty";
                        }
                        return seat;
                    }).ToList();
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
                    SectionName = "Económica",
                    Rows = economyRows
                }
            };

            int totalAsientos = firstClassRows.Sum(r => r.Seats.Count) + economyRows.Sum(r => r.Seats.Count);
            if (totalAsientos < 8 || totalAsientos > 853)
            {
                throw new Exception($"La configuración de asientos es inválida: total {totalAsientos} asientos. Debe estar entre 8 y 853.");
            }
            ViewBag.SeatConfig = seatConfig;

            var pasajerosList = _context.Pasajeros.ToList();
            var pasajerosDict = pasajerosList.ToDictionary(
                p => "P" + p.numero_pasaporte.ToString(),
                p => p.nombre_completo);
            ViewBag.Pasajeros = JsonConvert.SerializeObject(pasajerosDict);

            return View();
        }

        [HttpPost]
        public IActionResult BuscarPasajero([FromForm] string passport)
        {
            if (string.IsNullOrWhiteSpace(passport))
            {
                return Json(new { success = false, message = "El campo de pasaporte es obligatorio." });
            }
            if (!int.TryParse(passport, out int numeroPasaporte))
            {
                return Json(new { success = false, message = "Número de pasaporte inválido." });
            }

            var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajeroExistente != null)
            {
                return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
            }
            else
            {
                return Json(new { success = false, message = "Pasaporte no encontrado." });
            }
        }

        [HttpPost]
        public IActionResult ValidarPasajero([FromForm] string passport, [FromForm] string passenger)
        {
            if (string.IsNullOrWhiteSpace(passport) || string.IsNullOrWhiteSpace(passenger))
            {
                return Json(new { success = false, message = "Los campos de Pasaporte y Pasajero son obligatorios." });
            }

            if (!int.TryParse(passport, out int numeroPasaporte))
            {
                return Json(new { success = false, message = "Número de pasaporte inválido." });
            }

            var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajeroExistente != null)
            {
                return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
            }
            else
            {
                var nuevoPasajero = new Pasajero
                {
                    numero_pasaporte = numeroPasaporte,
                    nombre_completo = passenger
                };
                _context.Pasajeros.Add(nuevoPasajero);
                _context.SaveChanges();
                return Json(new { success = true, passenger = nuevoPasajero.nombre_completo });
            }
        }

        [HttpGet]
        public IActionResult GetFlights(DateTime date, string departure, string destination)
        {
            var flights = (from vuelo in _context.Set<Vuelo>()
                           join ruta in _context.Set<RutaComercial>() on vuelo.RutaId equals ruta.Id
                           join destinoOrigen in _context.Destinos on ruta.CiudadOrigenId equals destinoOrigen.Id
                           join destinoDestino in _context.Destinos on ruta.CiudadDestinoId equals destinoDestino.Id
                           where vuelo.FechaSalida.Date == date.Date &&
                                 destinoOrigen.descripcion_corta == departure &&
                                 destinoDestino.descripcion_corta == destination
                           select new
                           {
                               vuelo.CodigoVuelo,
                               OrigenDescripcion = destinoOrigen.descripcion_corta,
                               DestinoDescripcion = destinoDestino.descripcion_corta,
                               Hora = vuelo.FechaSalida.ToString("HH:mm"),
                              
                               Fecha = vuelo.FechaSalida.ToString("dd/MMM/yyyy", new CultureInfo("es-ES"))
                           }).ToList();

            var flightStrings = flights.Select(f =>
                $"{f.CodigoVuelo} {f.OrigenDescripcion} - {f.DestinoDescripcion} {f.Hora} {f.Fecha}"
            );
            return Json(flightStrings);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
