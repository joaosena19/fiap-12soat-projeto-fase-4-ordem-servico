using Infrastructure.Database;

namespace API.Configurations
{
    public static class DevelopmentDataSeeder
    {
        /// <summary>
        /// Popula o banco de dados com dados mock se estiver em ambiente de desenvolvimento e não executando testes de integração
        /// </summary>
        /// <param name="app">A aplicação web</param>
        public static void SeedIfDevelopment(WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
                return;

            Seed(app);
        }

        /// <summary>
        /// Popula o banco de dados com dados mock e não executando testes de integração
        /// </summary>
        /// <param name="app">A aplicação web</param>
        public static void Seed(WebApplication app)
        {
            try
            {
                if (IsIntegrationTest())
                    return;

                using var scope = app.Services.CreateScope();
                var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

                // MongoDB não precisa de migrations - as collections são criadas automaticamente
                // O seed de dados pode ser implementado aqui se necessário no futuro
                Console.WriteLine("MongoDB conectado - seed de dados pode ser implementado aqui");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Falha ao popular dados de desenvolvimento", ex);
            }
        }

        private static bool IsIntegrationTest()
        {
            // Verifica o assembly
            return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName?.ToUpper().Contains("TESTS") ?? false);
        }
    }
}
