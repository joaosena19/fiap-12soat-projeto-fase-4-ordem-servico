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

    protected BaseExternalHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
    /// Se não existir, gera um novo.
    /// </summary>
    protected void PropagateCorrelationId()
    {
        var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].ToString();
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

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
