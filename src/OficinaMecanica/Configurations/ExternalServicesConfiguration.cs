using Application.OrdemServico.Interfaces.External;
using Application.Contracts.Monitoramento;
using Infrastructure.ExternalServices;
using Infrastructure.ExternalServices.Http;
using Infrastructure.ExternalServices.Cadastro;
using Infrastructure.ExternalServices.Estoque;
using Infrastructure.Monitoramento;
using Microsoft.Extensions.Options;

namespace API.Configurations;

/// <summary>
/// Configuração para comunicação com serviços externos.
/// </summary>
public static class ExternalServicesConfiguration
{
    /// <summary>
    /// Configura os serviços externos e HttpClient para comunicação entre microsserviços.
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar HttpContextAccessor para acesso ao contexto HTTP
        services.AddHttpContextAccessor();

        // Registrar o Accessor de Correlation ID
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

        // Registrar o handler para propagar headers
        services.AddTransient<PropagateHeadersHandler>();

        // Vincular e validar settings de serviços externos
        var externalServicesSection = configuration.GetSection("ExternalServices");
        var settings = externalServicesSection.Get<ExternalServicesSettings>();
        
        if (settings == null || string.IsNullOrWhiteSpace(settings.CadastroBaseUrl))
            throw new InvalidOperationException("ExternalServices:CadastroBaseUrl não está configurado");
        
        if (string.IsNullOrWhiteSpace(settings.EstoqueBaseUrl))
            throw new InvalidOperationException("ExternalServices:EstoqueBaseUrl não está configurado");

        // Registrar HttpClient tipado para os serviços de Cadastros
        services.AddHttpClient<IClienteExternalService, CadastroHttpClientService>(client =>
        {
            client.BaseAddress = new Uri(settings.CadastroBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<PropagateHeadersHandler>();

        services.AddHttpClient<IServicoExternalService, CadastroHttpClientService>(client =>
        {
            client.BaseAddress = new Uri(settings.CadastroBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<PropagateHeadersHandler>();

        services.AddHttpClient<IVeiculoExternalService, CadastroHttpClientService>(client =>
        {
            client.BaseAddress = new Uri(settings.CadastroBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<PropagateHeadersHandler>();

        // Registrar HttpClient tipado para o serviço de Estoque
        services.AddHttpClient<IEstoqueExternalService, EstoqueHttpClientService>(client =>
        {
            client.BaseAddress = new Uri(settings.EstoqueBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<PropagateHeadersHandler>();

        return services;
    }
}
