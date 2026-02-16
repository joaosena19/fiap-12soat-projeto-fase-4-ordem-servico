using API.Configurations;
using Application.Contracts.Messaging;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Application.SharedHelpers;

namespace Tests.API.Configurations;

public class MessagingConfigurationTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public MessagingConfigurationTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging();
        _services.AddSingleton(MockLogger.CriarSimples());
        
        var configurationBuilder = new ConfigurationBuilder();
        _configuration = configurationBuilder.Build();
    }

    [Fact(DisplayName = "Deve registrar MessagePublisher quando chamado")]
    [Trait("API", "MessagingConfiguration")]
    public void AddMessaging_DeveRegistrarMessagePublisher_QuandoChamado()
    {
        // Arrange & Act
        _services.AddMessaging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var messagePublisher = serviceProvider.GetService<IEstoqueMessagePublisher>();
        messagePublisher.Should().NotBeNull("o IEstoqueMessagePublisher deve estar registrado");
    }

    [Fact(DisplayName = "Deve registrar MassTransit quando chamado")]
    [Trait("API", "MessagingConfiguration")]
    public void AddMessaging_DeveRegistrarMassTransit_QuandoChamado()
    {
        // Arrange & Act
        _services.AddMessaging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var busControl = serviceProvider.GetService<IBusControl>();
        busControl.Should().NotBeNull("o IBusControl do MassTransit deve estar registrado");
    }

    [Fact(DisplayName = "Deve registrar IBus quando chamado")]
    [Trait("API", "MessagingConfiguration")]
    public void AddMessaging_DeveRegistrarIBus_QuandoChamado()
    {
        // Arrange & Act
        _services.AddMessaging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var bus = serviceProvider.GetService<IBus>();
        bus.Should().NotBeNull("o IBus do MassTransit deve estar registrado");
    }

    [Fact(DisplayName = "Deve retornar ServiceCollection para fluent interface")]
    [Trait("API", "MessagingConfiguration")]
    public void AddMessaging_DeveRetornarServiceCollection_ParaFluentInterface()
    {
        // Arrange & Act
        var resultado = _services.AddMessaging(_configuration);

        // Assert
        resultado.Should().BeSameAs(_services, "deve retornar a mesma instância para permitir encadeamento");
    }
}
