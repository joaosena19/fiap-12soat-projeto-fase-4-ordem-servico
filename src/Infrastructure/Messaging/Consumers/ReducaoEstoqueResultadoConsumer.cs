using Application.Contracts.Messaging;
using Application.Contracts.Monitoramento;
using MassTransit;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer que recebe resultado da redução de estoque e executa compensação se necessário.
/// </summary>
public class ReducaoEstoqueResultadoConsumer : IConsumer<ReducaoEstoqueResultado>
{
    private readonly IAppLogger _logger;

    public ReducaoEstoqueResultadoConsumer(IAppLogger logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReducaoEstoqueResultado> context)
    {
        var msg = context.Message;

        if (msg.Sucesso)
        {
            _logger.LogInformation(
                "Redução de estoque confirmada para OS {OsId}. CorrelationId: {CorrelationId}",
                msg.OrdemServicoId, msg.CorrelationId);
            await Task.CompletedTask;
            return;
        }

        // TODO: IMPLEMENTAR COMPENSAÇÃO (será implementado na Phase F)
        _logger.LogWarning(
            "Falha na redução de estoque para OS {OsId}. Motivo: {Motivo}. Compensação será implementada na Phase F. CorrelationId: {CorrelationId}",
            msg.OrdemServicoId, msg.MotivoFalha, msg.CorrelationId);

        await Task.CompletedTask;
    }
}
