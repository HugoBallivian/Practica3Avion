using Microsoft.EntityFrameworkCore;
using AvionesDistribuidos.Models;

namespace AvionesDistribuidos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pais> Paises { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Destino> Destinos { get; set; }
        public DbSet<Pasajero> Pasajeros { get; set; }
        public DbSet<Vuelo> Vuelos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pasajero>().ToTable("Pasajero");
            modelBuilder.Entity<Destino>().ToTable("Destino");
            modelBuilder.Entity<Vuelo>().ToTable("Vuelo");

            modelBuilder.Entity<Ciudad>()
                .HasOne(c => c.Pais)
                .WithMany(p => p.Ciudades)
                .HasForeignKey(c => c.PaisId);
        }
    }
}
