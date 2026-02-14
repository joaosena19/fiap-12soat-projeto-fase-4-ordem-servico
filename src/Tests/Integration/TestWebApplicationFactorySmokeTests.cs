using Tests.Integration.Fixtures;
using Xunit;

namespace Tests.Integration;

/// <summary>
/// Smoke tests para verificar a inicialização do TestWebApplicationFactory
/// </summary>
public class TestWebApplicationFactorySmokeTests : IClassFixture<Mongo2GoFixture>
{
    private readonly Mongo2GoFixture _fixture;

    public TestWebApplicationFactorySmokeTests(Mongo2GoFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "TestWebApplicationFactory deve inicializar com sucesso")]
    public void TestWebApplicationFactory_DeveInicializarComSucesso()
    {
        // Arrange & Act
        using var factory = new TestWebApplicationFactory(_fixture);

        // Assert
        Assert.NotNull(factory);
    }

    [Fact(DisplayName = "TestWebApplicationFactory deve criar cliente HTTP não autenticado")]
    public void TestWebApplicationFactory_DeveCriarClienteNaoAutenticado()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_fixture);

        // Act
        var client = factory.CreateUnauthenticatedClient();

        // Assert
        Assert.NotNull(client);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    [Fact(DisplayName = "TestWebApplicationFactory deve criar cliente HTTP autenticado como admin")]
    public void TestWebApplicationFactory_DeveCriarClienteAutenticadoComoAdmin()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_fixture);

        // Act
        var client = factory.CreateAuthenticatedClient(isAdmin: true);

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
    }

    [Fact(DisplayName = "TestWebApplicationFactory deve criar cliente HTTP autenticado como cliente")]
    public void TestWebApplicationFactory_DeveCriarClienteAutenticadoComoCliente()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_fixture);
        var clienteId = Guid.NewGuid();

        // Act
        var client = factory.CreateAuthenticatedClient(isAdmin: false, clienteId: clienteId);

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
    }
}
