using Application.Contracts.Messaging;
using Infrastructure.Messaging.DTOs;
using MassTransit;

namespace Infrastructure.Messaging;

/// <summary>
/// Implementação do publisher de mensagens de estoque usando MassTransit com Amazon SQS.
/// Configurado com Time-To-Live (TTL) de 60 segundos para prevenir "ghost deductions".
/// </summary>
public class EstoqueMessagePublisher : IEstoqueMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private static readonly TimeSpan MessageTtl = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Inicializa uma nova instância de EstoqueMessagePublisher.
    /// </summary>
    /// <param name="publishEndpoint">Endpoint de publicação do MassTransit.</param>
    public EstoqueMessagePublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    /// <summary>
    /// Publica uma solicitação de redução de estoque com TTL de 60 segundos.
    /// O TTL atua como primeira linha de defesa contra "ghost deductions":
    /// - Se o serviço de Estoque estiver offline e não consumir a mensagem dentro de 60s,
    ///   a mensagem é descartada pelo SQS.
    /// - Trabalha em conjunto com o BackgroundService (SagaTimeoutBackgroundService) que
    ///   compensa ordens atingidas por timeout após 90s.
    /// </summary>
    /// <param name="solicitacao">Dados da solicitação de redução de estoque.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public async Task PublicarSolicitacaoReducaoAsync(ReducaoEstoqueSolicitacao solicitacao)
    {
        if (solicitacao == null)
            throw new ArgumentNullException(nameof(solicitacao));

        await _publishEndpoint.Publish(solicitacao, context =>
        {
            context.TimeToLive = MessageTtl;
        });
    }
}
