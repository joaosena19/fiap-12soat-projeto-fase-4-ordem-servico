using Domain.OrdemServico.Aggregates.OrdemServico;
using Infrastructure.Repositories.OrdemServico;
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
        /// Construtor para testes/mocks
        /// </summary>
        protected MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        /// <summary>
        /// Construtor vazio para mocks
        /// </summary>
        protected MongoDbContext()
        {
            _database = null!;
        }

        /// <summary>
        /// Coleção de Ordens de Serviço (usando Documents para persistência)
        /// </summary>
        public virtual IMongoCollection<OrdemServicoDocument> OrdensServico => 
            _database.GetCollection<OrdemServicoDocument>("ordens_servico");
    }
}
