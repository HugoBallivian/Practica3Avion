using AvionesDistribuidos.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AvionesDistribuidos.Data
{
    public class ConnectionResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConnectionResolver> _logger;

        public ConnectionResolver(IServiceProvider serviceProvider, ILogger<ConnectionResolver> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IDatabaseService GetDatabaseService(string continent)
        {
            _logger.LogInformation($"Solicitando conexión para continente: {continent}");

            return continent switch
            {
                "Asia" or "Oceania" => (IDatabaseService)_serviceProvider.GetService(typeof(MongoDbService)),
                "Europe" or "Africa" => (IDatabaseService)_serviceProvider.GetService(typeof(ApplicationDbContextEurope)),
                _ => (IDatabaseService)_serviceProvider.GetService(typeof(SqlServerService)),
            };
        }

        private IDatabaseService GetServiceWithLog(Type serviceType, string dbName)
        {
            _logger.LogInformation($"Conectando a base de datos: {dbName}");
            return (IDatabaseService)_serviceProvider.GetService(serviceType);
        }
    }
}
