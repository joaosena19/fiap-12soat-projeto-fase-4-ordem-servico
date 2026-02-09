namespace Application.Contracts.Monitoramento;

/// <summary>
/// Provedor para acesso ao Correlation ID da operação atual.
/// Permite recuperar o ID de rastreamento de forma transparente para as camadas superiores.
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Recupera o correlation ID da operação atual.
    /// Se não houver um ID no contexto atual (ex: fora de uma requisição HTTP), 
    /// gera um novo GUID para manter a rastreabilidade.
    /// </summary>
    /// <returns>O Guid de correlação.</returns>
    Guid GetCorrelationId();
}