using Application.Contracts.Monitoramento;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Monitoramento;

/// <summary>
/// Implementação do ICorrelationIdAccessor que extrai o ID do header HTTP ou gera um novo.
/// </summary>
public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public Guid GetCorrelationId()
    {
        var correlationIdStr = _httpContextAccessor.HttpContext?.Request.Headers[CorrelationIdHeader].ToString();

        if (string.IsNullOrEmpty(correlationIdStr) || !Guid.TryParse(correlationIdStr, out var correlationId))
        {
            // Se não encontrar ou não for GUID válido (ex: background service sem HTTP context), 
            // tenta buscar do LogContext que o middleware pode ter populado ou gera um novo.
            return Guid.NewGuid();
        }

        return correlationId;
    }
}