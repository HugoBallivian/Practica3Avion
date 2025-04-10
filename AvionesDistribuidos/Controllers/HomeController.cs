using AvionesDistribuidos.Data;
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

            var pasajerosList = _context.Pasajeros.ToList();
            var pasajerosDict = pasajerosList.ToDictionary(
                p => "P" + p.numero_pasaporte.ToString(),
                p => p.nombre_completo);
            ViewBag.Pasajeros = JsonConvert.SerializeObject(pasajerosDict);

            List<SeatConfiguration> seatConfig;

            if (TempData["SeatConfig"] != null)
            {
                // Usar configuración del avión
                var config = JsonConvert.DeserializeObject<dynamic>(TempData["SeatConfig"].ToString());

                char fcRowStart = 'A';
                char fcRowEnd = config.LastRow1;
                int fcColStart = 1;
                int fcColEnd = config.LastCol1;

                char econRowStart = 'A';
                char econRowEnd = config.LastRow2;
                int econColStart = config.LastCol1 + 1;
                int econColEnd = config.LastCol2;

                // Generar asientos primera clase
                var firstClassRows = GenerateSeatRows(fcRowStart, fcRowEnd, fcColStart, fcColEnd);

                // Generar asientos económicos
                var economyRows = GenerateSeatRows(econRowStart, econRowEnd, econColStart, econColEnd);

                seatConfig = new List<SeatConfiguration>
        {
            new SeatConfiguration { SectionName = "Primera Clase", Rows = firstClassRows },
            new SeatConfiguration { SectionName = "Económica", Rows = economyRows }
        };
            }
            else
            {
                // Configuración por defecto (como la que ya tenías)
                seatConfig = GetDefaultSeatConfiguration();
            }

            ViewBag.SeatConfig = seatConfig;

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

<<<<<<< Updated upstream
            var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajeroExistente != null)
            {
                return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
            }
            else
            {
                var nuevoPasajero = new Pasajero
=======
            // Buscar en la BD
            var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajeroExistente != null)
                return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
            else
                return Json(new { success = false, message = "Pasaporte no encontrado." });
        }

        /// <summary>
        /// Valida o crea un nuevo pasajero según pasaporte y nombre.
        /// </summary>
        //[HttpPost]
        //public IActionResult ValidarPasajero([FromForm] string passport, [FromForm] string passenger)
        //{
        //    // Validar campos
        //    if (string.IsNullOrWhiteSpace(passport) || string.IsNullOrWhiteSpace(passenger))
        //        return Json(new { success = false, message = "Los campos de Pasaporte y Pasajero son obligatorios." });

        //    if (!int.TryParse(passport, out int numeroPasaporte))
        //        return Json(new { success = false, message = "Número de pasaporte inválido." });

        //    // Si existe, devolvemos su nombre
        //    var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
        //    if (pasajeroExistente != null)
        //    {
        //        return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
        //    }
        //    else
        //    {
        //        // Si no existe, creamos uno nuevo en la BD
        //        var nuevoPasajero = new Pasajero
        //        {
        //            numero_pasaporte = numeroPasaporte,
        //            nombre_completo = passenger
        //        };
        //        _context.Pasajeros.Add(nuevoPasajero);
        //        _context.SaveChanges();

        //        return Json(new { success = true, passenger = nuevoPasajero.nombre_completo });
        //    }
        //}
        [HttpPost]
        public IActionResult ValidarPasajero(
            [FromForm] string passport,
            [FromForm] string passenger,
            [FromForm] int asientoId,
            [FromForm] string estado,
            [FromForm] string servidorOrigen)
        {
            if (string.IsNullOrWhiteSpace(passport) || string.IsNullOrWhiteSpace(passenger))
                return Json(new { success = false, message = "Los campos de Pasaporte y Pasajero son obligatorios." });

            if (!int.TryParse(passport, out int numeroPasaporte))
                return Json(new { success = false, message = "Número de pasaporte inválido." });

            // Buscar o crear pasajero
            var pasajero = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajero == null)
            {
                pasajero = new Pasajero
>>>>>>> Stashed changes
                {
                    numero_pasaporte = numeroPasaporte,
                    nombre_completo = passenger
                };
                _context.Pasajeros.Add(pasajero);
                _context.SaveChanges();
<<<<<<< Updated upstream
                return Json(new { success = true, passenger = nuevoPasajero.nombre_completo });
=======
>>>>>>> Stashed changes
            }

            // Convertir el estado a formato normalizado
            string estadoFinal = estado.Trim().ToLowerInvariant() switch
            {
                "reservado" => "Reservado",
                "vendido" => "Vendido",
                "devolucion" => "Devolucion",
                _ => "Disponible"
            };

            // Epoch timestamp
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // ACTUALIZAR registro existente (no se hace inserción)
            var estadoAsiento = _context.EstadosAsientosVuelo
                .FirstOrDefault(e => e.AsientoId == asientoId);

            if (estadoAsiento == null)
            {
                return Json(new { success = false, message = "No se encontró el estado del asiento en la base de datos." });
            }

            estadoAsiento.Estado = estadoFinal;
            estadoAsiento.PasajeroId = pasajero.numero_pasaporte;
            estadoAsiento.FechaHoraActualizacion = timestamp;
            estadoAsiento.ServidorOrigen = servidorOrigen;
            estadoAsiento.VectorClock = "{}"; // lógica de sincronización futura

            _context.SaveChanges();

            return Json(new { success = true, passenger = pasajero.nombre_completo });
        }

<<<<<<< Updated upstream
=======
        [HttpPost]
        public IActionResult ActualizarEstado(
            [FromForm] int asientoId,
            [FromForm] string estado,
            [FromForm] string servidorOrigen)
        {

            // Convertir el estado a formato normalizado
            string estadoFinal = estado.Trim().ToLowerInvariant() switch
            {
                "reservado" => "Reservado",
                "vendido" => "Vendido",
                "devolucion" => "Devolucion",
                _ => "Disponible"
            };

            // Epoch timestamp
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // ACTUALIZAR registro existente (no se hace inserción)
            var estadoAsiento = _context.EstadosAsientosVuelo
                .FirstOrDefault(e => e.AsientoId == asientoId);

            if (estadoAsiento == null)
            {
                return Json(new { success = false, message = "No se encontró el estado del asiento en la base de datos." });
            }

            estadoAsiento.Estado = estadoFinal;
            estadoAsiento.FechaHoraActualizacion = timestamp;
            estadoAsiento.ServidorOrigen = servidorOrigen;
            estadoAsiento.VectorClock = "{}"; // lógica de sincronización futura

            _context.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Devuelve la lista de vuelos según filtros de origen, destino y fecha.
        /// </summary>
>>>>>>> Stashed changes
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
                query = query.Where(x => x.d1.descripcion_corta == departure && x.d2.descripcion_corta == destination);
            }

            if (!string.IsNullOrWhiteSpace(date))
            {
                if (DateTime.TryParse(date, out DateTime flightDate))
                {
                    query = query.Where(x => x.v.FechaSalida.Date == flightDate.Date);
                }
                else
                {
                    return Json(new { success = false, message = "Fecha inválida." });
                }
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
            {
                return Json(new { success = false, message = "No se encontraron vuelos." });
            }

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

        [HttpPost]
        public IActionResult GetSeatConfiguration([FromForm] string flightCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(flightCode))
                {
                    return Json(new { success = false, message = "Código de vuelo inválido." });
                }

                // Obtener el avión asociado al vuelo
                var vuelo = _context.Vuelos
                    .Include(v => v.Avion)
                    .FirstOrDefault(v => v.CodigoVuelo == flightCode);

                if (vuelo == null || vuelo.Avion == null)
                {
                    return Json(new { success = false, message = "No se encontró configuración de avión para este vuelo." });
                }

                var avion = vuelo.Avion;

                // Guardar la configuración del avión en TempData para usarla en Index
                TempData["SeatConfig"] = JsonConvert.SerializeObject(new
                {
                    LastRow1 = avion.UltimaLetra1,
                    LastRow2 = avion.UltimaLetra2,
                    LastCol1 = avion.UltimoDigito1,
                    LastCol2 = avion.UltimoDigito2,
                    FlightId = vuelo.Id
                });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de asientos");
                return Json(new { success = false, message = "Error interno al generar asientos." });
            }
        }

        private List<SeatRow> GenerateSeatRows(char rowStart, char rowEnd, int colStart, int colEnd)
        {
            var rows = new List<SeatRow>();

            for (char row = rowStart; row <= rowEnd; row++)
            {
                var seats = new List<Seat>();

                for (int col = colStart; col <= colEnd; col++)
                {
                    seats.Add(new Seat { SeatId = $"{row}{col.ToString("D2")}" });
                }

                rows.Add(new SeatRow { RowLabel = row.ToString(), Seats = seats });
            }

            return rows;
        }

        private List<SeatConfiguration> GetDefaultSeatConfiguration()
        {
            // Tu configuración por defecto actual
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
        new SeatConfiguration { SectionName = "Económica", Rows = economyRows }
    };
        }
    }
}
