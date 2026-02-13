using Application.Contracts.Monitoramento;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Infrastructure.ExternalServices.Http;

/// <summary>
/// Handler HTTP que propaga automaticamente headers de autorização e correlation ID
/// para todas as requisições de serviços externos.
/// </summary>
public class PropagateHeadersHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public PropagateHeadersHandler(
        IHttpContextAccessor httpContextAccessor, 
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _correlationIdAccessor = correlationIdAccessor ?? throw new ArgumentNullException(nameof(correlationIdAccessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Propagar token de autorização se presente
        PropagateAuthToken(request);
        
        // Propagar correlation ID
        PropagateCorrelationId(request);
        
        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Propaga o token de autorização da requisição atual para a requisição HTTP externa.
    /// </summary>
    private void PropagateAuthToken(HttpRequestMessage request)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        
        if (!string.IsNullOrEmpty(authHeader))
        {
            var token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Propaga o correlation ID da requisição atual para a requisição HTTP externa.
    /// Utiliza o ICorrelationIdAccessor para garantir consistência.
    /// </summary>
    private void PropagateCorrelationId(HttpRequestMessage request)
    {
        var correlationId = _correlationIdAccessor.GetCorrelationId();
        
        // Remove existente se presente
        if (request.Headers.Contains("X-Correlation-ID"))
        {
            request.Headers.Remove("X-Correlation-ID");
        }
        
        request.Headers.Add("X-Correlation-ID", correlationId);
    }
}