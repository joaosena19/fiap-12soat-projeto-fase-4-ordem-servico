namespace Application.Contracts.Messaging;

/// <summary>
/// Interface para publicação de mensagens relacionadas ao estoque.
/// Utilizada pelo serviço de Ordem de Serviço para comunicação assíncrona com o serviço de Estoque.
/// </summary>
public interface IEstoqueMessagePublisher
{
    /// <summary>
    /// Publica uma solicitação de redução de estoque na fila Amazon SQS.
    /// A mensagem é configurada com Time-To-Live (TTL) de 60 segundos para prevenir "ghost deductions"
    /// caso o serviço de Estoque esteja offline ou indisponível.
    /// </summary>
    /// <param name="solicitacao">Dados da solicitação de redução de estoque.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task PublicarSolicitacaoReducaoAsync(ReducaoEstoqueSolicitacao solicitacao);
}
