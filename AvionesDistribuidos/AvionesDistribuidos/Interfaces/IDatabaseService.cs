using AvionesDistribuidos.Models;

namespace AvionesDistribuidos.Interfaces
{
    public interface IDatabaseService
    {
        Task<List<Pais>> GetPaisesAsync();
        Task InsertPasajeroAsync(Pasajero pasajero);


    }
}
