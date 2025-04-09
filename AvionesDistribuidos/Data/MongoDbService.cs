using AvionesDistribuidos.Interfaces;
using AvionesDistribuidos.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AvionesDistribuidos.Data
{
    public class MongoDbService : IDatabaseService
    {
        private readonly IMongoCollection<Pais> _paises;
        private readonly IMongoCollection<Pasajero> _pasajeros;

        public MongoDbService(IOptions<MongoDbSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);

            _paises = database.GetCollection<Pais>("Paises");
            _pasajeros = database.GetCollection<Pasajero>("Pasajeros");
        }

        public async Task<List<Pais>> GetPaisesAsync()
        {
            return await _paises.Find(_ => true).ToListAsync();
        }

        public async Task InsertPasajeroAsync(Pasajero pasajero)
        {
            await _pasajeros.InsertOneAsync(pasajero);
        }
    }
}
