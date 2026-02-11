using Application.Contracts.Monitoramento;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Monitoramento;

/// <summary>
/// Implementação do ICorrelationIdAccessor que extrai o ID do header HTTP.
/// Alinhado com CorrelationIdMiddleware: assume que o middleware já garantiu que
/// o header X-Correlation-ID sempre contém um GUID válido quando há contexto HTTP.
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

        // Em contexto HTTP, o middleware já garantiu que é um GUID válido
        if (!string.IsNullOrEmpty(correlationIdStr) && Guid.TryParse(correlationIdStr, out var correlationId))
        {
            return correlationId;
        }

        // Fallback para contextos sem HTTP (ex: background services, testes)
        // onde o middleware não foi executado
        return Guid.NewGuid();
    }
}