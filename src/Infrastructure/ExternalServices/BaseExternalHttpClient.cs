using Application.Contracts.Monitoramento;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using Shared.Exceptions;
using Shared.Enums;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Cliente HTTP base para comunicação com serviços externos.
/// Fornece métodos utilitários para tratamento de erros HTTP padronizado.
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
    /// Verifica se a resposta HTTP foi bem-sucedida ou lança exceção apropriada.
    /// - 5xx: DomainException com ErrorType.BadGateway (502)
    /// - 4xx: Relança como DomainException com detalhes
    /// - HttpRequestException: DomainException com ErrorType.BadGateway (502)
    /// </summary>
    /// <param name="response">Resposta HTTP a ser verificada</param>
    /// <param name="operation">Nome da operação para contexto de log</param>
    /// <param name="logger">Logger para registrar erros</param>
    /// <exception cref="DomainException">Para erros upstream (5xx) ou cliente (4xx)</exception>
    public static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, string operation, IAppLogger? logger = null)
    {
        if (response.IsSuccessStatusCode)
            return;

        var statusCode = response.StatusCode;
        var reasonPhrase = response.ReasonPhrase ?? "Unknown";
        var content = await response.Content.ReadAsStringAsync();

        if ((int)statusCode >= 500)
        {
            // 5xx - Erro do servidor upstream, mapear para BadGateway
            var message = $"Falha no serviço externo durante operação '{operation}': {statusCode} {reasonPhrase}";
            
            logger?.LogError(message + ". Conteúdo: {Content}", content);
            
            throw new DomainException(message, ErrorType.BadGateway);
        }
        else if ((int)statusCode >= 400)
        {
            // 4xx - Erro do cliente, logar como Information e relançar
            var message = $"Erro de cliente na operação '{operation}': {statusCode} {reasonPhrase}";
            
            logger?.LogInformation(message + ". Conteúdo: {Content}", content);
            
            // Manter como DomainException mas pode ser mapeado para o status original
            var errorType = statusCode switch
            {
                HttpStatusCode.NotFound => ErrorType.ResourceNotFound,
                HttpStatusCode.Unauthorized => ErrorType.Unauthorized,
                HttpStatusCode.Forbidden => ErrorType.NotAllowed,
                HttpStatusCode.Conflict => ErrorType.Conflict,
                HttpStatusCode.UnprocessableEntity => ErrorType.DomainRuleBroken,
                _ => ErrorType.InvalidInput
            };
            
            throw new DomainException(message, errorType);
        }
    }

    /// <summary>
    /// Executa uma operação HTTP e trata erros de rede/transporte.
    /// HttpRequestException é convertida para DomainException com ErrorType.BadGateway.
    /// </summary>
    /// <param name="httpOperation">Operação HTTP a ser executada</param>
    /// <param name="operationName">Nome da operação para contexto</param>
    /// <param name="logger">Logger para registrar erros</param>
    /// <returns>Resposta HTTP</returns>
    /// <exception cref="DomainException">Para erros de rede/transporte</exception>
    public static async Task<HttpResponseMessage> ExecuteHttpOperationAsync(
        Func<Task<HttpResponseMessage>> httpOperation, 
        string operationName, 
        IAppLogger? logger = null)
    {
        try
        {
            return await httpOperation();
        }
        catch (HttpRequestException ex)
        {
            var message = $"Erro de conectividade na operação '{operationName}': {ex.Message}";
            
            logger?.LogError(ex, message);
            
            throw new DomainException(message, ErrorType.BadGateway);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            var message = $"Timeout na operação '{operationName}': {ex.Message}";
            
            logger?.LogError(ex, message);
            
            throw new DomainException(message, ErrorType.BadGateway);
        }
    }

}
