using Infrastructure.Database;
using Mongo2Go;
using MongoDB.Driver;

namespace Tests.Integration.Fixtures;

/// <summary>
/// Fixture para gerenciar instância temporária do MongoDB usando Mongo2Go
/// </summary>
public class Mongo2GoFixture : IAsyncLifetime
{
    private MongoDbRunner? _runner;
    
    public string ConnectionString { get; private set; } = string.Empty;
    public string DatabaseName { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Configura mapeamentos do MongoDB antes de iniciar
        MongoDbMappingConfiguration.Configure();
        
        // Inicia o MongoDB temporário
        _runner = MongoDbRunner.Start();
        ConnectionString = _runner.ConnectionString;
        
        // Gera nome único do banco para evitar colisão entre execuções de testes
        DatabaseName = $"test_os_{Guid.NewGuid():N}";
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        try
        {
            // Limpa o banco de dados antes de parar o runner
            if (_runner != null && !string.IsNullOrEmpty(ConnectionString) && !string.IsNullOrEmpty(DatabaseName))
            {
                var client = new MongoClient(ConnectionString);
                await client.DropDatabaseAsync(DatabaseName);
            }
        }
        catch (Exception ex)
        {
            // Log mas não propaga exceção no cleanup
            Console.WriteLine($"Erro ao limpar banco de dados no DisposeAsync: {ex.Message}");
        }
        finally
        {
            // Para e descarta o runner do MongoDB
            _runner?.Dispose();
        }
    }
}
