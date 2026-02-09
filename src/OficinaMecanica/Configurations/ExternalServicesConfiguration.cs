using Application.OrdemServico.Interfaces.External;
using Application.Contracts.Monitoramento;
using Infrastructure.ExternalServices;
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

        // Configurar settings de serviços externos
        services.Configure<ExternalServicesSettings>(
            configuration.GetSection("ExternalServices"));

        // Registrar HttpClient tipado para o serviço de Cadastros
        services.AddHttpClient<CadastroHttpClientService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
            client.BaseAddress = new Uri(settings.CadastroBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Registrar as interfaces de serviço externo para o CadastroHttpClientService
        services.AddScoped<IClienteExternalService>(sp => 
            sp.GetRequiredService<CadastroHttpClientService>());
        services.AddScoped<IServicoExternalService>(sp => 
            sp.GetRequiredService<CadastroHttpClientService>());
        services.AddScoped<IVeiculoExternalService>(sp => 
            sp.GetRequiredService<CadastroHttpClientService>());

        // Registrar HttpClient tipado para o serviço de Estoque
        services.AddHttpClient<EstoqueHttpClientService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
            client.BaseAddress = new Uri(settings.EstoqueBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Registrar a interface de serviço externo para o EstoqueHttpClientService
        services.AddScoped<IEstoqueExternalService>(sp => 
            sp.GetRequiredService<EstoqueHttpClientService>());

        return services;
    }
}
