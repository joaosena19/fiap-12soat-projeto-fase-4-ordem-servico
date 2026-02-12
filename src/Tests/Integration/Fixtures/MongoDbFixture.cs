using Testcontainers.MongoDb;
using Infrastructure.Database;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Tests.Integration.Fixtures;

public class MongoDbFixture : IAsyncDisposable
{
    private readonly MongoDbContainer _mongoContainer;
    private readonly string _databaseName;

    public MongoDbFixture()
    {
        // Gera nome único para o banco de teste  
        _databaseName = $"test_ordem_servico_{Guid.NewGuid():N}";
        
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithPortBinding(27018, true) // Porta aleatória para evitar conflitos
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "testuser")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "testpass")
            .Build();
    }

    public string ConnectionString => _mongoContainer.GetConnectionString();
    public string DatabaseName => _databaseName;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
    }

    public MongoDbContext CreateContext()
    {
        var settings = Options.Create(new MongoDbSettings
        {
            ConnectionString = ConnectionString,
            DatabaseName = DatabaseName
        });
        
        return new MongoDbContext(settings);
    }

    public async ValueTask DisposeAsync()
    {
        if (_mongoContainer != null)
        {
            await _mongoContainer.DisposeAsync();
        }
    }
}