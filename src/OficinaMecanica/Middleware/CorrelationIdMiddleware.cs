using Serilog.Context;

namespace API.Middleware;

/// <summary>
/// Middleware que garante a propagação do header X-Correlation-ID em todas as requisições.
/// Se a requisição não contiver o header, gera um novo GUID como correlation ID.
/// O correlation ID é incluído na resposta e enriquecido no contexto de log do Serilog.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = EnsureValidCorrelationId(context);
        
        // Garantir que o header da resposta reflita o correlation ID válido usado
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// Garante que existe um correlation ID válido (GUID) no header da requisição.
    /// Se o header estiver ausente ou não for um GUID válido, gera um novo GUID e o substitui.
    /// </summary>
    private string EnsureValidCorrelationId(HttpContext context)
    {
        var existingHeader = context.Request.Headers[CorrelationIdHeader].ToString();
        
        // Se header existe e é um GUID válido, usa o existente
        if (!string.IsNullOrEmpty(existingHeader) && Guid.TryParse(existingHeader, out _))
        {
            return existingHeader;
        }
        
        // Caso contrário, gera novo GUID e substitui/adiciona ao header da requisição
        var newCorrelationId = Guid.NewGuid().ToString();
        
        if (context.Request.Headers.ContainsKey(CorrelationIdHeader))
        {
            context.Request.Headers.Remove(CorrelationIdHeader);
        }
        
        context.Request.Headers.Append(CorrelationIdHeader, newCorrelationId);
        return newCorrelationId;
    }
}
