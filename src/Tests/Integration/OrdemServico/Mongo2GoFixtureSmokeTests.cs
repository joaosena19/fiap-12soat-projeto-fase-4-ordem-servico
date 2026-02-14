using FluentAssertions;
using Infrastructure.Database;
using Infrastructure.Repositories.OrdemServico;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Tests.Integration.Fixtures;
using Tests.Integration.Helpers;
using OrdemServicoAggregate = global::Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Integration.OrdemServico;

/// <summary>
/// Testes de smoke para verificar funcionamento do Mongo2GoFixture e helpers
/// </summary>
[Trait("Category", "Integration")]
[Trait("Type", "Smoke")]
public class Mongo2GoFixtureSmokeTests : IClassFixture<Mongo2GoFixture>
{
    private readonly Mongo2GoFixture _fixture;

    public Mongo2GoFixtureSmokeTests(Mongo2GoFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Fixture deve inicializar com ConnectionString e DatabaseName válidos")]
    public void Fixture_DeveInicializarComPropriedadesValidas()
    {
        // Arrange & Act & Assert
        _fixture.ConnectionString.Should().NotBeNullOrEmpty();
        _fixture.DatabaseName.Should().NotBeNullOrEmpty();
        _fixture.DatabaseName.Should().StartWith("test_os_");
    }

    [Fact(DisplayName = "MongoDbContext deve conectar usando ConnectionString do fixture")]
    public void MongoDbContext_DeveConectarComFixture()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MongoDbSettings>(settings =>
        {
            settings.ConnectionString = _fixture.ConnectionString;
            settings.DatabaseName = _fixture.DatabaseName;
        });
        services.AddSingleton<MongoDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var context = serviceProvider.GetRequiredService<MongoDbContext>();

        // Assert
        context.Should().NotBeNull();
        context.OrdensServico.Should().NotBeNull();
    }

    [Fact(DisplayName = "Helper deve limpar coleção com sucesso")]
    public async Task Helper_DeveLimparColecaoComSucesso()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MongoDbSettings>(settings =>
        {
            settings.ConnectionString = _fixture.ConnectionString;
            settings.DatabaseName = _fixture.DatabaseName;
        });
        services.AddSingleton<MongoDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        await MongoOrdemServicoTestHelper.ClearAsync(serviceProvider);

        // Assert - não deve lançar exceção
        var context = serviceProvider.GetRequiredService<MongoDbContext>();
        var count = await context.OrdensServico.CountDocumentsAsync(FilterDefinition<OrdemServicoDocument>.Empty);
        count.Should().Be(0);
    }

    [Fact(DisplayName = "Helper deve inserir e buscar ordem de serviço por Id")]
    public async Task Helper_DeveInserirEBuscarPorId()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MongoDbSettings>(settings =>
        {
            settings.ConnectionString = _fixture.ConnectionString;
            settings.DatabaseName = _fixture.DatabaseName;
        });
        services.AddSingleton<MongoDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        await MongoOrdemServicoTestHelper.ClearAsync(serviceProvider);

        var veiculoId = Guid.NewGuid();
        var ordemServico = OrdemServicoAggregate.Criar(veiculoId);

        // Act
        await MongoOrdemServicoTestHelper.InsertSeedAsync(serviceProvider, ordemServico);
        var found = await MongoOrdemServicoTestHelper.FindByIdAsync(serviceProvider, ordemServico.Id);

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(ordemServico.Id);
        found.VeiculoId.Should().Be(veiculoId);
    }

    [Fact(DisplayName = "Helper deve buscar ordem de serviço por Codigo")]
    public async Task Helper_DeveBuscarPorCodigo()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MongoDbSettings>(settings =>
        {
            settings.ConnectionString = _fixture.ConnectionString;
            settings.DatabaseName = _fixture.DatabaseName;
        });
        services.AddSingleton<MongoDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        await MongoOrdemServicoTestHelper.ClearAsync(serviceProvider);

        var veiculoId = Guid.NewGuid();
        var ordemServico = OrdemServicoAggregate.Criar(veiculoId);
        var codigo = ordemServico.Codigo.Valor;

        // Act
        await MongoOrdemServicoTestHelper.InsertSeedAsync(serviceProvider, ordemServico);
        var found = await MongoOrdemServicoTestHelper.FindByCodigoAsync(serviceProvider, codigo);

        // Assert
        found.Should().NotBeNull();
        found!.Codigo.Valor.Should().Be(codigo);
        found.VeiculoId.Should().Be(veiculoId);
    }

    [Fact(DisplayName = "FindById deve retornar null quando ordem de serviço não existe")]
    public async Task FindById_DeveRetornarNullQuandoNaoExiste()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MongoDbSettings>(settings =>
        {
            settings.ConnectionString = _fixture.ConnectionString;
            settings.DatabaseName = _fixture.DatabaseName;
        });
        services.AddSingleton<MongoDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        await MongoOrdemServicoTestHelper.ClearAsync(serviceProvider);

        var idNaoExistente = Guid.NewGuid();

        // Act
        var found = await MongoOrdemServicoTestHelper.FindByIdAsync(serviceProvider, idNaoExistente);

        // Assert
        found.Should().BeNull();
    }

    [Fact(DisplayName = "FindByCodigo deve retornar null quando código não existe")]
    public async Task FindByCodigo_DeveRetornarNullQuandoNaoExiste()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MongoDbSettings>(settings =>
        {
            settings.ConnectionString = _fixture.ConnectionString;
            settings.DatabaseName = _fixture.DatabaseName;
        });
        services.AddSingleton<MongoDbContext>();
        var serviceProvider = services.BuildServiceProvider();

        await MongoOrdemServicoTestHelper.ClearAsync(serviceProvider);

        var codigoInexistente = "OS-999999";

        // Act
        var found = await MongoOrdemServicoTestHelper.FindByCodigoAsync(serviceProvider, codigoInexistente);

        // Assert
        found.Should().BeNull();
    }
}
