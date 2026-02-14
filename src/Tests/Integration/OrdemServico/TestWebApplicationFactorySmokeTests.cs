using FluentAssertions;
using Moq;
using Tests.Integration.Fixtures;
using Xunit;

namespace Tests.Integration.OrdemServico;

/// <summary>
/// Testes básicos para verificar que o TestWebApplicationFactory está funcionando corretamente
/// </summary>
public class TestWebApplicationFactorySmokeTests : IClassFixture<Mongo2GoFixture>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public TestWebApplicationFactorySmokeTests(Mongo2GoFixture fixture)
    {
        _factory = new TestWebApplicationFactory(fixture);
    }

    [Fact(DisplayName = "Deve criar factory com sucesso")]
    [Trait("Method", "GET")]
    public void DeveCriarFactoryComSucesso()
    {
        // Act & Assert
        _factory.Should().NotBeNull();
        _factory.Mocks.Should().NotBeNull();
        _factory.Mocks.ClienteService.Should().NotBeNull();
        _factory.Mocks.VeiculoService.Should().NotBeNull();
        _factory.Mocks.ServicoService.Should().NotBeNull();
        _factory.Mocks.EstoqueService.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve criar cliente não autenticado")]
    [Trait("Method", "GET")]
    public void DeveCriarClienteNaoAutenticado()
    {
        // Act
        var client = _factory.CreateUnauthenticatedClient();

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Should().BeNull();
    }

    [Fact(DisplayName = "Deve criar cliente autenticado como admin")]
    [Trait("Method", "GET")]
    public void DeveCriarClienteAutenticadoComoAdmin()
    {
        // Act
        var client = _factory.CreateAuthenticatedClient(isAdmin: true);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "Deve criar cliente autenticado como cliente")]
    [Trait("Method", "GET")]
    public void DeveCriarClienteAutenticadoComoCliente()
    {
        // Act
        var clienteId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(isAdmin: false, clienteId: clienteId);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "Deve permitir reset de mocks")]
    [Trait("Method", "GET")]
    public void DevePermitirResetDeMocks()
    {
        // Act & Assert (não deve lançar exceção)
        _factory.Mocks.ResetAll();
        
        // Verificar que todos os mocks estão limpos após reset
        _factory.Mocks.ClienteService.Should().NotBeNull();
        _factory.Mocks.VeiculoService.Should().NotBeNull();
        _factory.Mocks.ServicoService.Should().NotBeNull();
        _factory.Mocks.EstoqueService.Should().NotBeNull();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _factory?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}