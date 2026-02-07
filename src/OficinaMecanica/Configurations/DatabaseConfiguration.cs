using Infrastructure.Database;

namespace API.Configurations
{
    /// <summary>
    /// Configuração do banco de dados
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Configura o MongoDB
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <param name="configuration">Configuração da aplicação</param>
        /// <returns>Coleção de serviços configurada</returns>
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Configura as opções do MongoDB a partir da seção de configuração
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));

            // Registra o contexto do MongoDB como singleton
            services.AddSingleton<MongoDbContext>();

            var mongoSettings = configuration.GetSection("MongoDB");
            var connectionString = mongoSettings["ConnectionString"];
            var databaseName = mongoSettings["DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("MongoDB ConnectionString não foi configurada");

            if (string.IsNullOrEmpty(databaseName))
                throw new InvalidOperationException("MongoDB DatabaseName não foi configurado");

            Console.WriteLine($"Conectado ao MongoDB: Database={databaseName}");

            return services;
        }
    }
}
