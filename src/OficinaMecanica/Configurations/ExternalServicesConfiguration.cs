using Infrastructure.ExternalServices;

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

        // Configurar settings de serviços externos
        services.Configure<ExternalServicesSettings>(
            configuration.GetSection("ExternalServices"));

        return services;
    }
}
