using Domain.OrdemServico.Aggregates.OrdemServico;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Database
{
    /// <summary>
    /// Contexto do MongoDB para acesso às coleções
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Construtor do contexto MongoDB
        /// </summary>
        /// <param name="settings">Configurações do MongoDB</param>
        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var mongoSettings = settings.Value;
            
            if (string.IsNullOrEmpty(mongoSettings.ConnectionString))
                throw new InvalidOperationException("MongoDB ConnectionString não foi configurada");
            
            if (string.IsNullOrEmpty(mongoSettings.DatabaseName))
                throw new InvalidOperationException("MongoDB DatabaseName não foi configurado");
            
            var client = new MongoClient(mongoSettings.ConnectionString);
            _database = client.GetDatabase(mongoSettings.DatabaseName);
        }

        /// <summary>
        /// Coleção de Ordens de Serviço
        /// </summary>
        public IMongoCollection<OrdemServico> OrdensServico => 
            _database.GetCollection<OrdemServico>("ordens_servico");
    }
}
