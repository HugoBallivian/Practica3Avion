using Microsoft.EntityFrameworkCore;
using AvionesDistribuidos.Models;
using AvionesDistribuidos.Interfaces;


namespace AvionesDistribuidos.Data
{
    public class ApplicationDbContext : DbContext, IDatabaseService
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pais> Paises { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Destino> Destinos { get; set; }
        public DbSet<Pasajero> Pasajeros { get; set; }

        // Agrega estas dos líneas
        public DbSet<Vuelo> Vuelos { get; set; }
        public DbSet<RutaComercial> RutasComerciales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pasajero>().ToTable("Pasajero");

            // Mapeo de la entidad Destino a la tabla "Destino"
            modelBuilder.Entity<Destino>().ToTable("Destino");

            modelBuilder.Entity<Ciudad>()
                .HasOne(c => c.Pais)
                .WithMany(p => p.Ciudades)
                .HasForeignKey(c => c.PaisId);
        }

        public async Task<List<Pais>> GetPaisesAsync()
        {
            return await Paises.ToListAsync();
        }

        public async Task InsertPasajeroAsync(Pasajero pasajero)
        {
            await Pasajeros.AddAsync(pasajero);
            await SaveChangesAsync();
        }

    }
}
