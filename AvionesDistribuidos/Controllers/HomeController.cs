﻿using AvionesDistribuidos.Data;
using AvionesDistribuidos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
                "B-101 Ucrania(Kiev) - Bolivia(Cochabamba) 18:55 20/Abr/2025",
                "A-205 España(Madrid) - Argentina(Buenos Aires) 09:30 15/Abr/2025"
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

        [HttpPost]
        public IActionResult GetFlights([FromForm] string departure, [FromForm] string destination, [FromForm] string date)
        {
            if (string.IsNullOrWhiteSpace(departure) || string.IsNullOrWhiteSpace(destination) || string.IsNullOrWhiteSpace(date))
            {
                return Json(new { success = false, message = "Faltan datos para la búsqueda de vuelos." });
            }

            if (!DateTime.TryParse(date, out DateTime flightDate))
            {
                return Json(new { success = false, message = "Fecha inválida." });
            }

            // Se realiza el join entre Vuelo, RutaComercial y Destino para obtener la descripción de origen y destino.
            var vuelos = (from v in _context.Set<Vuelo>()
                          join r in _context.Set<RutaComercial>() on v.RutaId equals r.Id
                          join d1 in _context.Destinos on r.CiudadOrigenId equals d1.Id
                          join d2 in _context.Destinos on r.CiudadDestinoId equals d2.Id
                          where d1.descripcion_corta == departure &&
                                d2.descripcion_corta == destination &&
                                v.FechaSalida.Date == flightDate.Date
                          select new
                          {
                              code = v.CodigoVuelo,
                              departureDesc = d1.descripcion_corta,
                              destinationDesc = d2.descripcion_corta,
                              time = v.FechaSalida.ToString("HH:mm"),
                              date = v.FechaSalida.ToString("dd/MMM/yyyy")
                          }).ToList();

            return Json(new { success = true, flights = vuelos });
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
