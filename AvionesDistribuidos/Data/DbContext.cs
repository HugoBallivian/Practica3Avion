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
        public DbSet<RutaComercial> RutasComerciales { get; set; }
        public DbSet<Avion> Aviones { get; set; }
        public DbSet<Asiento> Asientos { get; set; }
        public DbSet<EstadoAsientosVuelo> EstadoAsientosVuelo { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pasajero>().ToTable("Pasajero");
            modelBuilder.Entity<Destino>().ToTable("Destino");
            modelBuilder.Entity<Vuelo>().ToTable("Vuelo");
            modelBuilder.Entity<RutaComercial>().ToTable("RutaComercial");
            modelBuilder.Entity<Avion>().ToTable("Avion");
            modelBuilder.Entity<Asiento>().ToTable("Asiento");
            modelBuilder.Entity<EstadoAsientosVuelo>().ToTable("estado_asientos_vuelo");

            modelBuilder.Entity<Ciudad>()
                .HasOne(c => c.Pais)
                .WithMany(p => p.Ciudades)
                .HasForeignKey(c => c.PaisId);

            modelBuilder.Entity<RutaComercial>()
                .HasOne(r => r.CiudadOrigen)
                .WithMany()
                .HasForeignKey(r => r.CiudadOrigenId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<RutaComercial>()
                .HasOne(r => r.CiudadDestino)
                .WithMany()
                .HasForeignKey(r => r.CiudadDestinoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Vuelo>()
                .HasOne(v => v.Avion)
                .WithMany()
                .HasForeignKey(v => v.AvionId);

            modelBuilder.Entity<Vuelo>()
                .HasOne(v => v.Ruta)
                .WithMany()
                .HasForeignKey(v => v.RutaId);

            modelBuilder.Entity<Asiento>()
                .HasOne(a => a.Vuelo)
                .WithMany(v => v.Asientos)
                .HasForeignKey(a => a.VueloId);

            modelBuilder.Entity<EstadoAsientosVuelo>()
                .HasOne(e => e.Vuelo)
                .WithMany()
                .HasForeignKey(e => e.VueloId);

            modelBuilder.Entity<EstadoAsientosVuelo>()
                .HasOne(e => e.Asiento)
                .WithMany()
                .HasForeignKey(e => e.AsientoId);

            modelBuilder.Entity<EstadoAsientosVuelo>()
                .HasOne(e => e.Pasajero)
                .WithMany()
                .HasForeignKey(e => e.PasajeroId);

            modelBuilder.Entity<EstadoAsientosVuelo>()
                .HasIndex(e => new { e.VueloId, e.AsientoId })
                .IsUnique();
        }
    }
}
