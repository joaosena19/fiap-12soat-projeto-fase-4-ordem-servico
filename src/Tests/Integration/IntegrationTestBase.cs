using Tests.Integration.Fixtures;
using Tests.Integration.Helpers;
using Tests.Integration.Mocks;
using Tests.Integration.OrdemServico.Builders;
using Xunit;
using OrdemServicoAggregate = global::Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Integration;

/// <summary>
/// Classe base abstrata para testes de integração fornecendo infraestrutura compartilhada
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<Mongo2GoFixture>, IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient ClientAdmin;
    protected readonly HttpClient ClientUnauth;

    protected IntegrationTestBase(Mongo2GoFixture fixture)
    {
        Factory = new TestWebApplicationFactory(fixture);
        ClientAdmin = Factory.CreateAuthenticatedClient(isAdmin: true);
        ClientUnauth = Factory.CreateUnauthenticatedClient();
    }

    /// &lt;summary&gt;
    /// Inicialização assíncrona - limpa o banco antes de cada teste
    /// &lt;/summary&gt;
    public virtual async Task InitializeAsync()
    {
        await MongoOrdemServicoTestHelper.ClearAsync(Factory.Services);
    }

    /// &lt;summary&gt;
    /// Limpeza assíncrona - limpa o banco e dispõe recursos após cada teste
    /// &lt;/summary&gt;
    public virtual async Task DisposeAsync()
    {
        await MongoOrdemServicoTestHelper.ClearAsync(Factory.Services);
        ClientAdmin?.Dispose();
        ClientUnauth?.Dispose();
        Factory?.Dispose();
    }

    /// &lt;summary&gt;
    /// Insere uma ordem de serviço no banco para usar como seed nos testes
    /// &lt;/summary&gt;
    protected async Task SeedOrdemServicoAsync(OrdemServicoAggregate ordemServico)
    {
        await MongoOrdemServicoTestHelper.InsertSeedAsync(Factory.Services, ordemServico);
    }

    /// <summary>
    /// Busca ordem de serviço por ID do banco
    /// </summary>
    protected async Task<OrdemServicoAggregate?> FindOrdemServicoByIdAsync(Guid id)
    {
        return await MongoOrdemServicoTestHelper.FindByIdAsync(Factory.Services, id);
    }

    /// <summary>
    /// Busca ordem de serviço por código do banco
    /// </summary>
    protected async Task<OrdemServicoAggregate?> FindOrdemServicoByCodigoAsync(string codigo)
    {
        return await MongoOrdemServicoTestHelper.FindByCodigoAsync(Factory.Services, codigo);
    }

    /// &lt;summary&gt;
    /// Cria um cliente autenticado com configurações específicas
    /// &lt;/summary&gt;
    protected HttpClient CreateAuthenticatedClient(bool isAdmin = true, Guid? clienteId = null, Guid? usuarioId = null)
    {
        return Factory.CreateAuthenticatedClient(isAdmin, clienteId, usuarioId);
    }

    /// <summary>
    /// Acesso aos mocks de serviços externos para configuração em testes específicos
    /// </summary>
    protected MockExternalServices Mocks => Factory.Mocks;
}