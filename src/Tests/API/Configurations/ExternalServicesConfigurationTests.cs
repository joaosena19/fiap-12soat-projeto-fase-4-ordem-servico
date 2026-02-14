using API.Configurations;
using Application.Contracts.Monitoramento;
using Application.OrdemServico.Interfaces.External;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Application.SharedHelpers;

namespace Tests.API.Configurations;

public class ExternalServicesConfigurationTests
{
    private readonly IServiceCollection _services;

    public ExternalServicesConfigurationTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging();
        _services.AddSingleton(MockLogger.CriarSimples());
    }

    [Fact(DisplayName = "Deve lançar InvalidOperationException quando CadastroBaseUrl está ausente")]
    [Trait("API", "ExternalServicesConfiguration")]
    public void AddExternalServices_DeveLancarInvalidOperationException_QuandoCadastroBaseUrlAusente()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalServices:CadastroBaseUrl"] = "",
            ["ExternalServices:EstoqueBaseUrl"] = "http://estoque:8080"
        });
        var configuration = configurationBuilder.Build();

        // Act
        var acao = () => _services.AddExternalServices(configuration);

        // Assert
        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*CadastroBaseUrl*");
    }

    [Fact(DisplayName = "Deve lançar InvalidOperationException quando EstoqueBaseUrl está ausente")]
    [Trait("API", "ExternalServicesConfiguration")]
    public void AddExternalServices_DeveLancarInvalidOperationException_QuandoEstoqueBaseUrlAusente()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalServices:CadastroBaseUrl"] = "http://cadastro:8080",
            ["ExternalServices:EstoqueBaseUrl"] = ""
        });
        var configuration = configurationBuilder.Build();

        // Act
        var acao = () => _services.AddExternalServices(configuration);

        // Assert
        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*EstoqueBaseUrl*");
    }

    [Fact(DisplayName = "Deve lançar InvalidOperationException quando ExternalServices não está configurado")]
    [Trait("API", "ExternalServicesConfiguration")]
    public void AddExternalServices_DeveLancarInvalidOperationException_QuandoConfigAusente()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        // Act
        var acao = () => _services.AddExternalServices(configuration);

        // Assert
        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*CadastroBaseUrl*");
    }

    [Fact(DisplayName = "Deve registrar serviços quando configuração é válida")]
    [Trait("API", "ExternalServicesConfiguration")]
    public void AddExternalServices_DeveRegistrarServicos_QuandoConfigValida()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalServices:CadastroBaseUrl"] = "http://cadastro:8080",
            ["ExternalServices:EstoqueBaseUrl"] = "http://estoque:8080"
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.AddExternalServices(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ICorrelationIdAccessor>().Should().NotBeNull("CorrelationIdAccessor deve estar registrado");
        serviceProvider.GetService<IClienteExternalService>().Should().NotBeNull("ClienteExternalService deve estar registrado");
        serviceProvider.GetService<IServicoExternalService>().Should().NotBeNull("ServicoExternalService deve estar registrado");
        serviceProvider.GetService<IVeiculoExternalService>().Should().NotBeNull("VeiculoExternalService deve estar registrado");
        serviceProvider.GetService<IEstoqueExternalService>().Should().NotBeNull("EstoqueExternalService deve estar registrado");
    }

    [Fact(DisplayName = "Deve retornar ServiceCollection para fluent interface")]
    [Trait("API", "ExternalServicesConfiguration")]
    public void AddExternalServices_DeveRetornarServiceCollection_ParaFluentInterface()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalServices:CadastroBaseUrl"] = "http://cadastro:8080",
            ["ExternalServices:EstoqueBaseUrl"] = "http://estoque:8080"
        });
        var configuration = configurationBuilder.Build();

        // Act
        var resultado = _services.AddExternalServices(configuration);

        // Assert
        resultado.Should().BeSameAs(_services, "deve retornar a mesma instância para permitir encadeamento");
    }
}
