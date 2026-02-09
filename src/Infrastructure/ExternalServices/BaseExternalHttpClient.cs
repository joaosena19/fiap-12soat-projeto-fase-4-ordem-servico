using Application.Contracts.Monitoramento;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Cliente HTTP base para comunicação com serviços externos.
/// Propaga automaticamente o token de autenticação e o correlation ID da requisição atual.
/// </summary>
public abstract class BaseExternalHttpClient
{
    protected readonly HttpClient _httpClient;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ICorrelationIdAccessor _correlationIdAccessor;

    protected BaseExternalHttpClient(
        HttpClient httpClient, 
        IHttpContextAccessor httpContextAccessor,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _correlationIdAccessor = correlationIdAccessor ?? throw new ArgumentNullException(nameof(correlationIdAccessor));
    }

    /// <summary>
    /// Propaga o token de autorização da requisição atual para o HttpClient.
    /// </summary>
    protected void PropagateAuthToken()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        
        if (!string.IsNullOrEmpty(authHeader))
        {
            var token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Propaga o correlation ID da requisição atual para o HttpClient.
    /// Utiliza o ICorrelationIdAccessor para garantir consistência.
    /// </summary>
    protected void PropagateCorrelationId()
    {
        var correlationId = _correlationIdAccessor.GetCorrelationId().ToString();

        // Remove existente antes de adicionar
        if (_httpClient.DefaultRequestHeaders.Contains("X-Correlation-ID"))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
        }

        _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
    }

    /// <summary>
    /// Propaga token de autenticação e correlation ID em uma única chamada.
    /// </summary>
    protected void PropagateHeaders()
    {
        PropagateAuthToken();
        PropagateCorrelationId();
    }
}
