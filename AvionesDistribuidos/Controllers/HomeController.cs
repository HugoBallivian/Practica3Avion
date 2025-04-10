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
        // Logger para registrar información y errores
        private readonly ILogger<HomeController> _logger;
        // Contexto de la base de datos (Entity Framework)
        private readonly ApplicationDbContext _context;
        private readonly ConnectionResolver _resolver;
        /// <summary>
        /// Constructor: inyecta el logger y el contexto de BD
        /// </summary>
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ConnectionResolver resolver)
        {
            _logger = logger;
            _context = context;
            _resolver = resolver;
        }

        /// <summary>
        /// Acción principal que muestra la vista de reservación
        /// </summary>
        public async Task<IActionResult> Index(string country)
        {
            _logger.LogInformation($"Método Index ejecutado con continente: {country}");

            // 1) Cargar lista de países desde un JSON estático en wwwroot/data
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "country-by-continent.json");
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            // Deserializamos a lista de CountryJson
            var data = JsonConvert.DeserializeObject<Dictionary<string, List<CountryJson>>>(jsonData);

            ViewBag.Countries = data;
            string continent = "Default";

            // Mapear CountryJson a CountryViewModel (solo Name y Capital)
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
                // Primer país como default
                country = countriesGrouped.First().Countries.First();
                continent = data.FirstOrDefault(x => x.Value.Any(p => p.country == country)).Key ?? "Default";
            }

            ViewBag.SelectedCountry = country;
            ViewBag.Countries = countriesGrouped;

            // 2) Lista de vuelos de ejemplo (solo texto) para poblar el dropdown inicial
            var flights = new List<string>
            {
                "B-101 Ucrania(Kiev) - Bolivia(Cochabamba) 18:55 20/Abr/2025",
                "A-205 España(Madrid) - Argentina(Buenos Aires) 09:30 15/Abr/2025"
            };
            ViewBag.Flights = flights;

            // 3) Si hay info de vuelo en TempData (de llamada previa), la pasamos a la vista
            if (TempData["FlightInfo"] != null)
            {
                ViewBag.FlightInfo = JsonConvert.DeserializeObject<dynamic>(TempData["FlightInfo"].ToString());
            }

            // 4) Obtener lista de destinos desde la base de datos
            var destinosList = _context.Destinos.ToList();
            ViewBag.Destinos = destinosList;

            // 5) Obtener lista de pasajeros, convertir a diccionario pasaporte→nombre y serializar a JSON
            var pasajerosList = _context.Pasajeros.ToList();
            var pasajerosDict = pasajerosList.ToDictionary(
                p => "P" + p.numero_pasaporte.ToString(),
                p => p.nombre_completo);
            ViewBag.Pasajeros = JsonConvert.SerializeObject(pasajerosDict);

            // 6) Configuración de asientos: si hay en TempData, generarla dinámicamente; si no, usar default
            List<SeatConfiguration> seatConfig;

            if (TempData["SeatConfig"] != null)
            {
                // Leer configuración guardada en TempData
                var config = JsonConvert.DeserializeObject<dynamic>(TempData["SeatConfig"].ToString());
                TempData.Keep("SeatConfig"); // mantener para la siguiente petición
                ViewBag.FlightId = (int)config.FlightId;

                // Primera clase: filas A→LastRow1, columnas 1→LastCol1
                char fcRowStart = 'A';
                char fcRowEnd = config.LastRow1;
                int fcColStart = 1;
                int fcColEnd = config.LastCol1;

                // Económica: filas A→LastRow2, columnas LastCol1+1→LastCol2
                char econRowStart = 'A';
                char econRowEnd = config.LastRow2;
                int econColStart = config.LastCol1 + 1;
                int econColEnd = config.LastCol2;

                // Generar listas de SeatRow para cada sección
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
                // Sin configuración previa, usar configuración por defecto
                seatConfig = GetDefaultSeatConfiguration();
            }

            ViewBag.SeatConfig = seatConfig;


            _logger.LogInformation($"Solicitando conexión para continente: {continent}");
            var databaseService = _resolver.GetDatabaseService(continent);
            var paises = await databaseService.GetPaisesAsync();
            ViewBag.Paises = paises;

            // Devolver la vista con todos los ViewBag poblados
            return View();
        }

        /// <summary>
        /// Busca un pasajero por número de pasaporte.
        /// </summary>
        [HttpPost]
        public IActionResult BuscarPasajero([FromForm] string passport)
        {
            // Validar que venga algo
            if (string.IsNullOrWhiteSpace(passport))
                return Json(new { success = false, message = "El campo de pasaporte es obligatorio." });

            // Validar que sea numérico
            if (!int.TryParse(passport, out int numeroPasaporte))
                return Json(new { success = false, message = "Número de pasaporte inválido." });

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
        [HttpPost]
        public IActionResult ValidarPasajero([FromForm] string passport, [FromForm] string passenger)
        {
            // Validar campos
            if (string.IsNullOrWhiteSpace(passport) || string.IsNullOrWhiteSpace(passenger))
                return Json(new { success = false, message = "Los campos de Pasaporte y Pasajero son obligatorios." });

            if (!int.TryParse(passport, out int numeroPasaporte))
                return Json(new { success = false, message = "Número de pasaporte inválido." });

            // Si existe, devolvemos su nombre
            var pasajeroExistente = _context.Pasajeros.FirstOrDefault(p => p.numero_pasaporte == numeroPasaporte);
            if (pasajeroExistente != null)
            {
                return Json(new { success = true, passenger = pasajeroExistente.nombre_completo });
            }
            else
            {
                // Si no existe, creamos uno nuevo en la BD
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

        /// <summary>
        /// Devuelve la lista de vuelos según filtros de origen, destino y fecha.
        /// </summary>
        [HttpPost]
        public IActionResult GetFlights([FromForm] string departure, [FromForm] string destination, [FromForm] string date)
        {
            // Construir query base uniendo tablas Vuelo, RutaComercial y Destinos
            var query = from v in _context.Set<Vuelo>()
                        join r in _context.Set<RutaComercial>() on v.RutaId equals r.Id
                        join d1 in _context.Destinos on r.CiudadOrigenId equals d1.Id
                        join d2 in _context.Destinos on r.CiudadDestinoId equals d2.Id
                        select new { v, d1, d2 };

            // Filtrar por departure + destination si vienen
            if (!string.IsNullOrWhiteSpace(departure) && !string.IsNullOrWhiteSpace(destination))
            {
                query = query.Where(x => x.d1.descripcion_corta == departure
                                       && x.d2.descripcion_corta == destination);
            }

            // Filtrar por fecha si viene
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

            // Proyectar al formato que espera el cliente
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

        /// <summary>
        /// Obtiene configuración de asientos para un vuelo y la guarda en TempData.
        /// </summary>
        [HttpPost]
        public IActionResult GetSeatConfiguration([FromForm] string flightCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(flightCode))
                    return Json(new { success = false, message = "Código de vuelo inválido." });

                // Cargar vuelo con datos de avión y ruta (incluyendo ciudades)
                var vuelo = _context.Vuelos
                    .Include(v => v.Avion)
                    .Include(v => v.Ruta)
                        .ThenInclude(r => r.CiudadOrigen)
                    .Include(v => v.Ruta)
                        .ThenInclude(r => r.CiudadDestino)
                    .FirstOrDefault(v => v.CodigoVuelo == flightCode);

                if (vuelo == null || vuelo.Avion == null || vuelo.Ruta == null)
                    return Json(new { success = false, message = "No se encontró configuración para este vuelo." });

                // Preparar objeto con info de vuelo para mostrar en vista
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

                // Guardar en TempData para que persista hasta la siguiente petición
                TempData["FlightInfo"] = JsonConvert.SerializeObject(flightInfo);
                TempData.Keep("FlightInfo");

                // Guardar parámetros de configuración de asientos
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

        /// <summary>
        /// Devuelve el partial view con el mapa de asientos.
        /// </summary>
        public IActionResult SeatMapPartial()
        {
            List<SeatConfiguration> seatConfig;

            if (TempData["SeatConfig"] != null)
            {
                var config = JsonConvert.DeserializeObject<dynamic>(TempData["SeatConfig"].ToString());
                TempData.Keep("SeatConfig");

                // Obtener ID de vuelo para cruzar con base de datos
                int idVuelo = config.FlightId;

                // Obtener los asientos de la base de datos para ese vuelo
                var asientosDB = _context.Asientos
                    .Where(a => a.VueloId == idVuelo)
                    .ToDictionary(a => a.NumeroAsiento, a => a.Id);

                // Obtener los precios de los asientos de la base de datos para ese vuelo
                var preciosAsientosDB = _context.Asientos
                    .Where(a => a.VueloId == idVuelo)
                    .ToDictionary(a => a.NumeroAsiento, a => a.Precio);

                // Obtener los estados de los asientos de la base de datos para ese vuelo
                var estadosDB = _context.EstadosAsientosVuelo
                    .AsNoTracking()
                    .Where(e => e.VueloId == idVuelo && e.Estado != null)
                    .Select(e => new { e.AsientoId, e.Estado })
                    .ToDictionary(e => e.AsientoId, e => e.Estado ?? "Disponible");

                // Mismos cálculos de filas y columnas que en Index()
                char fcRowStart = 'A';
                char fcRowEnd = config.LastRow1;
                int fcColStart = 1;
                int fcColEnd = config.LastCol1;
                char econRowStart = 'A';
                char econRowEnd = config.LastRow2;
                int econColStart = config.LastCol1 + 1;
                int econColEnd = config.LastCol2;

                var firstClassRows = GenerateSeatRows(fcRowStart, fcRowEnd, fcColStart, fcColEnd, asientosDB, preciosAsientosDB, estadosDB);
                var economyRows = GenerateSeatRows(econRowStart, econRowEnd, econColStart, econColEnd, asientosDB, preciosAsientosDB, estadosDB);

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

            // Devolver partial "_SeatMapPartial" con la lista de SeatConfiguration
            return PartialView("_SeatMapPartial", seatConfig);
        }

        /// <summary>
        /// Devuelve el partial view con la información del vuelo.
        /// </summary>
        [HttpGet]
        public IActionResult FlightInfoPartial()
        {
            if (TempData["FlightInfo"] != null)
                TempData.Keep("FlightInfo");

            return PartialView("_FlightInfoPartial");
        }

        /// <summary>
        /// Vista Privacy (estática).
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Maneja la vista de error, inyectando RequestId.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        #region Métodos auxiliares para generar configuración de asientos

        /// <summary>
        /// Genera las filas y asientos entre rowStart→rowEnd y colStart→colEnd.
        /// </summary>
        private List<SeatRow> GenerateSeatRows(char rowStart, char rowEnd, int colStart, int colEnd)
        {
            var rows = new List<SeatRow>();
            for (char row = rowStart; row <= rowEnd; row++)
            {
                var seats = new List<Seat>();
                for (int col = colStart; col <= colEnd; col++)
                {
                    // Formato SeatId: letra + dos dígitos (ej. A01)
                    seats.Add(new Seat { SeatId = $"{row}{col:D2}" });
                }
                rows.Add(new SeatRow { RowLabel = row.ToString(), Seats = seats });
            }
            return rows;
        }

        private List<SeatRow> GenerateSeatRows(char rowStart, char rowEnd, int colStart, int colEnd, Dictionary<string, int> asientosDB, Dictionary<string, decimal> preciosAsientosDB, Dictionary<int?, string> estadosDB)
        {
            var rows = new List<SeatRow>();
            for (char row = rowStart; row <= rowEnd; row++)
            {
                var seats = new List<Seat>();
                for (int col = colStart; col <= colEnd; col++)
                {
                    var seatId = $"{row}{col:D2}"; // Ej: A01, B12

                    var seat = new Seat
                    {
                        SeatId = seatId,
                        //State = 0 // estado inicial libre
                    };

                    if (asientosDB.TryGetValue(seatId, out var idAsiento))
                    {
                        seat.DatabaseId = idAsiento;
                    }
                    if (preciosAsientosDB.TryGetValue(seatId, out var priceAsiento))
                    {
                        seat.Price = priceAsiento;
                    }
                    if (estadosDB.TryGetValue(idAsiento, out var estado))
                    {
                        seat.State = estado;
                    }
                    else
                    {
                        seat.State = "Disponible"; // estado inicial libre
                    }

                        seats.Add(seat);
                }

                rows.Add(new SeatRow { RowLabel = row.ToString(), Seats = seats });
            }

            return rows;
        }


        /// <summary>
        /// Configuración por defecto: Primera Clase A–F columnas 1–3, Económica A–F columnas 4–29.
        /// </summary>
        private List<SeatConfiguration> GetDefaultSeatConfiguration()
        {
            // Parámetros por defecto
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

    }
}
