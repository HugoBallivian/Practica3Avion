using AvionesDistribuidos.Models;
using Microsoft.EntityFrameworkCore;

namespace AvionesDistribuidos.Data
{
    public class ApplicationDbContextEurope : DbContext
    {
        public ApplicationDbContextEurope(DbContextOptions<ApplicationDbContextEurope> options) : base(options)
        {
        }

        public DbSet<Pais> Paises { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Destino> Destinos { get; set; }
        public DbSet<Pasajero> Pasajeros { get; set; }
        public DbSet<Vuelo> Vuelos { get; set; }
        public DbSet<RutaComercial> RutasComerciales { get; set; }
    }
}
