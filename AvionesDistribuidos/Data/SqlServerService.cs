using AvionesDistribuidos.Interfaces;
using AvionesDistribuidos.Models;


namespace AvionesDistribuidos.Data
{
    public class SqlServerService : IDatabaseService
    {
        private readonly ApplicationDbContext _context;

        public SqlServerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pais>> GetPaisesAsync()
        {
            return await Task.FromResult(_context.Paises.ToList());
        }

        public async Task InsertPasajeroAsync(Pasajero pasajero)
        {
            _context.Pasajeros.Add(pasajero);
            await _context.SaveChangesAsync();
        }
    }
}
