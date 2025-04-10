using AvionesDistribuidos.Data;
using AvionesDistribuidos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MaxMind.GeoIP2.Model;

namespace AvionesDistribuidos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ConnectionResolver _resolver;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ConnectionResolver resolver)
        {
            _logger = logger;
            _context = context;
            _resolver = resolver;
        }

        public async Task<IActionResult> Index(string country)
        {
            _logger.LogInformation($"Método Index ejecutado con continente: {country}");

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "country-by-continent.json");
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<Dictionary<string, List<CountryJson>>>(jsonData);

            ViewBag.Countries = data;
            string continent = "Default";

            var countriesGrouped = data.Select(c => new
            {
                Continent = c.Key,
                Countries = c.Value.Select(x => x.country).ToList()
            }).ToList();

            if (!string.IsNullOrEmpty(country))
            {
                continent = data.FirstOrDefault(x => x.Value.Any(c => c.country == country)).Key;
                TempData["Message"] = $"Conexión cambiada a la base de datos de {continent}";
                _logger.LogInformation($"Solicitando conexión para continente: {continent}");
            }
            else
            {
                country = countriesGrouped.First().Countries.First();
                continent = data.FirstOrDefault(x => x.Value.Any(p => p.country == country)).Key ?? "Default";
            }

            ViewBag.SelectedCountry = country;
            ViewBag.Countries = countriesGrouped;

            var flights = new List<string>
            {
                "B-101 Ucrania(Kiev) - Bolivia(Cochabamba) 18:55 20/Abr/2025",
                "A-205 España(Madrid) - Argentina(Buenos Aires) 09:30 15/Abr/2025"
            };
            ViewBag.Flights = flights;

            if (TempData["FlightInfo"] != null)
            {
                ViewBag.FlightInfo = JsonConvert.DeserializeObject<dynamic>(TempData["FlightInfo"].ToString());
            }

            var destinosList = _context.Destinos.ToList();
            ViewBag.Destinos = destinosList;

            var pasajerosList = _context.Pasajeros.ToList();
            var pasajerosDict = pasajerosList.ToDictionary(
                p => "P" + p.numero_pasaporte.ToString(),
                p => p.nombre_completo);
            ViewBag.Pasajeros = JsonConvert.SerializeObject(pasajerosDict);

            List<SeatConfiguration> seatConfig;
            if (TempData["SeatConfig"] != null)
            {
                var config = JsonConvert.DeserializeObject<dynamic>(TempData["SeatConfig"].ToString());
                TempData.Keep("SeatConfig");
                ViewBag.FlightId = (int)config.FlightId;

                char fcRowStart = 'A';
                char fcRowEnd = config.LastRow1;
                int fcColStart = 1;
                int fcColEnd = config.LastCol1;
                char econRowStart = 'A';
                char econRowEnd = config.LastRow2;
                int econColStart = config.LastCol1 + 1;
                int econColEnd = config.LastCol2;

                var firstClassRows = GenerateSeatRows(fcRowStart, fcRowEnd, fcColStart, fcColEnd);
                var economyRows = GenerateSeatRows(econRowStart, econRowEnd, econColStart, econColEnd);

                seatConfig = new List<SeatConfiguration>
                {
                    new SeatConfiguration { SectionName = "Primera Clase", Rows = firstClassRows },
                    new SeatConfiguration { SectionName = "Económica",    Rows = economyRows   }
                };
            }
            else
            {
                seatConfig = GetDefaultSeatConfiguration();
            }

            ViewBag.SeatConfig = seatConfig;

            _logger.LogInformation($"Solicitando conexión para continente: {continent}");
            var databaseService = _resolver.GetDatabaseService(continent);
            var paises = await databaseService.GetPaisesAsync();
            ViewBag.Paises = paises;

            return View();
        }

        [HttpPost]
        public IActionResult BuscarPasajero([FromForm] string passport)
        {
            if (string.IsNullOrWhiteSpace(passport))
                return Json(new { success = false, message = "El campo de pasaporte es obligatorio." });

            if (!int.TryParse(passport, out int numeroPasaporte))
                return Json(new { success = false, message = "Número de pasaporte inválido." });

            var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajeroExistente != null)
                return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
            else
                return Json(new { success = false, message = "Pasaporte no encontrado." });
        }

        [HttpPost]
        public IActionResult ValidarPasajero([FromForm] string passport, [FromForm] string passenger)
        {
            if (string.IsNullOrWhiteSpace(passport) || string.IsNullOrWhiteSpace(passenger))
                return Json(new { success = false, message = "Los campos de Pasaporte y Pasajero son obligatorios." });

            if (!int.TryParse(passport, out int numeroPasaporte))
                return Json(new { success = false, message = "Número de pasaporte inválido." });

            var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajeroExistente != null)
                return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
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
            var query = from v in _context.Set<Vuelo>()
                        join r in _context.Set<RutaComercial>() on v.RutaId equals r.Id
                        join d1 in _context.Destinos on r.CiudadOrigenId equals d1.Id
                        join d2 in _context.Destinos on r.CiudadDestinoId equals d2.Id
                        select new { v, d1, d2 };

            if (!string.IsNullOrWhiteSpace(departure) && !string.IsNullOrWhiteSpace(destination))
            {
                query = query.Where(x => x.d1.descripcion_corta == departure
                                       && x.d2.descripcion_corta == destination);
            }

            if (!string.IsNullOrWhiteSpace(date))
            {
                if (DateTime.TryParse(date, out DateTime flightDate))
                    query = query.Where(x => x.v.FechaSalida.Date == flightDate.Date);
                else
                    return Json(new { success = false, message = "Fecha inválida." });
            }

            var vuelos = query.Select(x => new
            {
                code = x.v.CodigoVuelo,
                departureDesc = x.d1.descripcion_corta,
                destinationDesc = x.d2.descripcion_corta,
                time = x.v.FechaSalida.ToString("HH:mm"),
                date = x.v.FechaSalida.ToString("dd/MMM/yyyy")
            }).ToList();

            if (vuelos.Count == 0)
                return Json(new { success = false, message = "No se encontraron vuelos." });

            return Json(new { success = true, flights = vuelos });
        }

        [HttpPost]
        public IActionResult GetSeatConfiguration([FromForm] string flightCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(flightCode))
                    return Json(new { success = false, message = "Código de vuelo inválido." });

                var vuelo = _context.Vuelos
                    .Include(v => v.Avion)
                    .Include(v => v.Ruta).ThenInclude(r => r.CiudadOrigen)
                    .Include(v => v.Ruta).ThenInclude(r => r.CiudadDestino)
                    .FirstOrDefault(v => v.CodigoVuelo == flightCode);

                if (vuelo == null || vuelo.Avion == null || vuelo.Ruta == null)
                    return Json(new { success = false, message = "No se encontró configuración para este vuelo." });

                var flightInfo = new
                {
                    CodigoVuelo = vuelo.CodigoVuelo,
                    Origen = vuelo.Ruta.CiudadOrigen.Nombre,
                    Destino = vuelo.Ruta.CiudadDestino.Nombre,
                    FechaSalida = vuelo.FechaSalida.ToString("dd/MMM/yyyy"),
                    HoraSalida = vuelo.FechaSalida.ToString("HH:mm"),
                    FechaLlegada = vuelo.FechaLlegada.ToString("dd/MMM/yyyy"),
                    HoraLlegada = vuelo.FechaLlegada.ToString("HH:mm")
                };

                TempData["FlightInfo"] = JsonConvert.SerializeObject(flightInfo);
                TempData.Keep("FlightInfo");

                var avion = vuelo.Avion;
                TempData["SeatConfig"] = JsonConvert.SerializeObject(new
                {
                    LastRow1 = avion.UltimaLetra1,
                    LastRow2 = avion.UltimaLetra2,
                    LastCol1 = avion.UltimoDigito1,
                    LastCol2 = avion.UltimoDigito2,
                    FlightId = vuelo.Id
                });
                TempData.Keep("SeatConfig");

                return Json(new { success = true, flightInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de asientos");
                return Json(new { success = false, message = "Error interno al generar asientos." });
            }
        }

        public IActionResult SeatMapPartial()
        {
            List<SeatConfiguration> seatConfig;
            if (TempData["SeatConfig"] != null)
            {
                var config = JsonConvert.DeserializeObject<dynamic>(TempData["SeatConfig"].ToString());
                TempData.Keep("SeatConfig");
                int idVuelo = config.FlightId;

                var asientosDB = _context.Asientos
                    .Where(a => a.VueloId == idVuelo)
                    .ToDictionary(a => a.NumeroAsiento, a => a.Id);

                var preciosAsientosDB = _context.Asientos
                    .Where(a => a.VueloId == idVuelo)
                    .ToDictionary(a => a.NumeroAsiento, a => a.Precio);

                var estadosDB = _context.EstadosAsientosVuelo
                    .Where(e => e.VueloId == idVuelo && e.Estado != null)
                    .Select(e => new { e.AsientoId, e.Estado })
                    .ToDictionary(e => e.AsientoId, e => e.Estado ?? "Disponible");

                var pasaporteAsientosDB = _context.EstadosAsientosVuelo
                    .AsNoTracking()
                    .Where(e => e.VueloId == idVuelo && e.PasajeroId != null)
                    .ToDictionary(e => e.AsientoId, e => e.PasajeroId);

                var pasajeroAsientosDB = _context.EstadosAsientosVuelo
                    .AsNoTracking()
                    .Where(e => e.VueloId == idVuelo && e.PasajeroId != null)
                    .Include(e => e.Pasajero)
                    .ToDictionary(e => e.AsientoId, e => e.Pasajero!.nombre_completo!);

                char fcRowStart = 'A';
                char fcRowEnd = config.LastRow1;
                int fcColStart = 1;
                int fcColEnd = config.LastCol1;
                char econRowStart = 'A';
                char econRowEnd = config.LastRow2;
                int econColStart = config.LastCol1 + 1;
                int econColEnd = config.LastCol2;

                var firstClassRows = GenerateSeatRows(fcRowStart, fcRowEnd, fcColStart, fcColEnd, asientosDB, preciosAsientosDB, estadosDB, pasaporteAsientosDB, pasajeroAsientosDB);
                var economyRows = GenerateSeatRows(econRowStart, econRowEnd, econColStart, econColEnd, asientosDB, preciosAsientosDB, estadosDB, pasaporteAsientosDB, pasajeroAsientosDB);

                seatConfig = new List<SeatConfiguration>
                {
                    new SeatConfiguration { SectionName = "Primera Clase", Rows = firstClassRows },
                    new SeatConfiguration { SectionName = "Económica",    Rows = economyRows    }
                };
            }
            else
            {
                seatConfig = GetDefaultSeatConfiguration();
            }

            return PartialView("_SeatMapPartial", seatConfig);
        }

        [HttpGet]
        public IActionResult FlightInfoPartial()
        {
            if (TempData["FlightInfo"] != null)
                TempData.Keep("FlightInfo");

            return PartialView("_FlightInfoPartial");
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        #region Métodos auxiliares

        private List<SeatRow> GenerateSeatRows(char rowStart, char rowEnd, int colStart, int colEnd)
        {
            var rows = new List<SeatRow>();
            for (char row = rowStart; row <= rowEnd; row++)
            {
                var seats = new List<Seat>();
                for (int col = colStart; col <= colEnd; col++)
                    seats.Add(new Seat { SeatId = $"{row}{col:D2}" });
                rows.Add(new SeatRow { RowLabel = row.ToString(), Seats = seats });
            }
            return rows;
        }

        private List<SeatRow> GenerateSeatRows(char rowStart, char rowEnd, int colStart, int colEnd,
            Dictionary<string, int> asientosDB,
            Dictionary<string, decimal> preciosAsientosDB,
            Dictionary<int?, string> estadosDB,
            Dictionary<int?, int?> pasaporteAsientosDB,
            Dictionary<int?, string> pasajeroAsientosDB)
        {
            var rows = new List<SeatRow>();
            for (char row = rowStart; row <= rowEnd; row++)
            {
                var seats = new List<Seat>();
                for (int col = colStart; col <= colEnd; col++)
                {
                    var seatId = $"{row}{col:D2}";
                    var seat = new Seat { SeatId = seatId };

                    if (asientosDB.TryGetValue(seatId, out var idAsiento))
                        seat.DatabaseId = idAsiento;
                    if (preciosAsientosDB.TryGetValue(seatId, out var priceAsiento))
                        seat.Price = priceAsiento;
                    if (estadosDB.TryGetValue(idAsiento, out var estado))
                        seat.State = estado;
                    else
                        seat.State = "Disponible";
                    if (pasaporteAsientosDB.TryGetValue(idAsiento, out var pasaporte))
                        seat.Pasaporte = pasaporte;
                    if (pasajeroAsientosDB.TryGetValue(idAsiento, out var nombre))
                        seat.PasajeroNombre = nombre;

                    seats.Add(seat);
                }
                rows.Add(new SeatRow { RowLabel = row.ToString(), Seats = seats });
            }
            return rows;
        }

        private List<SeatConfiguration> GetDefaultSeatConfiguration()
        {
            char fcRowStart = 'A';
            char fcRowEnd = 'F';
            int fcColStart = 1;
            int fcColEnd = 3;
            char econRowStart = 'A';
            char econRowEnd = 'F';
            int econColStart = 4;
            int econColEnd = 29;

            var firstClassRows = GenerateSeatRows(fcRowStart, fcRowEnd, fcColStart, fcColEnd);
            var economyRows = GenerateSeatRows(econRowStart, econRowEnd, econColStart, econColEnd);

            return new List<SeatConfiguration>
            {
                new SeatConfiguration { SectionName = "Primera Clase", Rows = firstClassRows },
                new SeatConfiguration { SectionName = "Económica",    Rows = economyRows    }
            };
        }

        #endregion

        /// <summary>
        /// Guarda o actualiza el estado de un asiento en la tabla estado_asientos_vuelo
        /// </summary>
        [HttpPost]
        public IActionResult SaveSeatState([FromForm] int vueloId,
                                           [FromForm] string seatId,
                                           [FromForm] string estado,
                                           [FromForm] int? pasajeroId,
                                           [FromForm] string servidorOrigen,
                                           [FromForm] string vectorClockJson)
        {
            try
            {
                var registro = _context.EstadosAsientosVuelo
                    .FirstOrDefault(e => e.VueloId == vueloId && e.AsientoId ==
                        _context.Asientos.First(a => a.NumeroAsiento == seatId && a.VueloId == vueloId).Id);

                if (registro == null)
                {
                    registro = new EstadoAsientosVuelo
                    {
                        VueloId = vueloId,
                        AsientoId = _context.Asientos.First(a => a.NumeroAsiento == seatId && a.VueloId == vueloId).Id,
                        PasajeroId = pasajeroId,
                        Estado = estado,
                        ServidorOrigen = servidorOrigen,
                        VectorClock = vectorClockJson,
                        FechaHoraActualizacion = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };
                    _context.EstadosAsientosVuelo.Add(registro);
                }
                else
                {
                    registro.Estado = estado;
                    registro.PasajeroId = pasajeroId;
                    registro.ServidorOrigen = servidorOrigen;
                    registro.VectorClock = vectorClockJson;
                    registro.FechaHoraActualizacion = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    _context.EstadosAsientosVuelo.Update(registro);
                }

                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar estado de asiento");
                return Json(new { success = false, message = "Error interno al guardar estado." });
            }
        }
    }
}
