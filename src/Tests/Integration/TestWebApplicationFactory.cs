using Application.Contracts.Messaging;
using Application.Contracts.Messaging.DTOs;
using Application.OrdemServico.Interfaces.External;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using Tests.Integration.Fixtures;
using Tests.Integration.Mocks;

namespace Tests.Integration;

/// <summary>
/// WebApplicationFactory customizada para testes de integração usando Mongo2Go.
/// Substitui dependências externas por mocks e desabilita serviços em background.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Mongo2GoFixture _mongoFixture;
    
    public MockExternalServices Mocks { get; }

    public TestWebApplicationFactory(Mongo2GoFixture mongoFixture)
    {
        _mongoFixture = mongoFixture ?? throw new ArgumentNullException(nameof(mongoFixture));
        Mocks = new MockExternalServices();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // IMPORTANTE: UseSetting precisa vir ANTES de ConfigureAppConfiguration
        // para garantir que as configurações sejam aplicadas cedo o suficiente
        builder.UseSetting("MongoDB:ConnectionString", _mongoFixture.ConnectionString);
        builder.UseSetting("MongoDB:DatabaseName", _mongoFixture.DatabaseName);
        builder.UseSetting("Jwt:Key", JwtTestConstants.Key);
        builder.UseSetting("Jwt:Issuer", JwtTestConstants.Issuer);
        builder.UseSetting("Jwt:Audience", JwtTestConstants.Audience);
        builder.UseSetting("Jwt:ExpiresInMinutes", "60");
        builder.UseSetting("Webhook:HmacSecret", TestHmacUtils.TestHmacSecret);
        builder.UseSetting("ExternalServices:CadastroBaseUrl", "http://localhost:5000");
        builder.UseSetting("ExternalServices:EstoqueBaseUrl", "http://localhost:5001");
        
        // Configurar appsettings em memória (camada adicional)
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var overrides = new Dictionary<string, string>
            {
                // MongoDB settings
                ["MongoDB:ConnectionString"] = _mongoFixture.ConnectionString,
                ["MongoDB:DatabaseName"] = _mongoFixture.DatabaseName,
                
                // JWT settings
                ["Jwt:Key"] = JwtTestConstants.Key,
                ["Jwt:Issuer"] = JwtTestConstants.Issuer,
                ["Jwt:Audience"] = JwtTestConstants.Audience,
                ["Jwt:ExpiresInMinutes"] = "60",
                
                // HMAC settings para webhooks
                ["Webhook:HmacSecret"] = TestHmacUtils.TestHmacSecret,
                
                // Desabilitar logs verbosos em testes
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                
                // Configurações dummy para external services (não serão usadas nos mocks)
                ["ExternalServices:CadastroBaseUrl"] = "http://localhost:5000",
                ["ExternalServices:EstoqueBaseUrl"] = "http://localhost:5001"
            };
            
            config.AddInMemoryCollection(overrides!);
        });

        // Configurar serviços de teste
        builder.ConfigureTestServices(services =>
        {
            // IMPORTANTE: Remover MassTransit completamente antes de substituir outros serviços
            RemoveMassTransitServices(services);
            
            // Substituir serviços externos com mocks
            ReplaceMockServices(services);
            
            // Substituir message publisher por no-op
            ReplaceMessagePublisher(services);
            
            // Remover background services restantes
            RemoveBackgroundServices(services);
        });

        base.ConfigureWebHost(builder);
    }

    private void RemoveMassTransitServices(IServiceCollection services)
    {
        // Remover todos os serviços relacionados ao MassTransit
        var massTransitServices = services
            .Where(descriptor =>
            {
                var serviceTypeName = descriptor.ServiceType.FullName ?? "";
                var implTypeName = descriptor.ImplementationType?.FullName ?? 
                                  descriptor.ImplementationInstance?.GetType().FullName ?? "";
                                  
                return serviceTypeName.Contains("MassTransit", StringComparison.OrdinalIgnoreCase) ||
                       implTypeName.Contains("MassTransit", StringComparison.OrdinalIgnoreCase) ||
                       serviceTypeName.Contains("Consumer", StringComparison.OrdinalIgnoreCase) ||
                       implTypeName.Contains("ReducaoEstoqueResultadoConsumer", StringComparison.OrdinalIgnoreCase) ||
                       serviceTypeName.Contains("IBus", StringComparison.OrdinalIgnoreCase) ||
                       serviceTypeName.Contains("IPublishEndpoint", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        foreach (var service in massTransitServices)
        {
            services.Remove(service);
        }
    }

    private void ReplaceMockServices(IServiceCollection services)
    {
        // Remover implementações reais dos serviços externos
        var descriptorsToRemove = services
            .Where(d => d.ServiceType == typeof(IClienteExternalService) ||
                       d.ServiceType == typeof(IVeiculoExternalService) ||
                       d.ServiceType == typeof(IServicoExternalService) ||
                       d.ServiceType == typeof(IEstoqueExternalService))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }

        // Registrar mocks
        services.AddScoped<IClienteExternalService>(_ => Mocks.ClienteService.Object);
        services.AddScoped<IVeiculoExternalService>(_ => Mocks.VeiculoService.Object);
        services.AddScoped<IServicoExternalService>(_ => Mocks.ServicoService.Object);
        services.AddScoped<IEstoqueExternalService>(_ => Mocks.EstoqueService.Object);
    }

    private void ReplaceMessagePublisher(IServiceCollection services)
    {
        // Remover implementação real do message publisher
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEstoqueMessagePublisher));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Registrar no-op implementation
        services.AddScoped<IEstoqueMessagePublisher, NoOpEstoqueMessagePublisher>();
    }

    private void RemoveBackgroundServices(IServiceCollection services)
    {
        // Remover hosted services restantes (incluindo SagaTimeoutBackgroundService)
        var hostedServicesToRemove = services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
            .Where(descriptor =>
            {
                var implType = descriptor.ImplementationType?.FullName ?? 
                               descriptor.ImplementationInstance?.GetType().FullName ?? 
                               descriptor.ImplementationFactory?.GetType().FullName ?? "";
                
                return implType.Contains("SagaTimeoutBackgroundService", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        foreach (var service in hostedServicesToRemove)
        {
            services.Remove(service);
        }
    }

    /// <summary>
    /// Cria um cliente HTTP não autenticado
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Cria um cliente HTTP autenticado com token JWT válido
    /// </summary>
    /// <param name="isAdmin">Se true, cria token com role Administrador. Se false, role Cliente</param>
    /// <param name="clienteId">ID do cliente (opcional, para role Cliente)</param>
    /// <param name="usuarioId">ID do usuário (opcional, será gerado se não fornecido)</param>
    public HttpClient CreateAuthenticatedClient(bool isAdmin = true, Guid? clienteId = null, Guid? usuarioId = null)
    {
        var client = CreateClient();
        var token = TestAuthExtensions.CreateJwtToken(isAdmin, clienteId, usuarioId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Implementação no-op do IEstoqueMessagePublisher para testes
    /// </summary>
    private class NoOpEstoqueMessagePublisher : IEstoqueMessagePublisher
    {
        public Task PublicarSolicitacaoReducaoAsync(ReducaoEstoqueSolicitacao solicitacao)
        {
            // No-op: não faz nada em testes
            return Task.CompletedTask;
        }
    }
}
