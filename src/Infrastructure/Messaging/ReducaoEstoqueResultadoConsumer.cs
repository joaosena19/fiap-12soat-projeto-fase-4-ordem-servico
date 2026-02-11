using Application.Contracts.Gateways;
using Application.Contracts.Messaging;
using Application.Contracts.Messaging.DTOs;
using Application.Contracts.Monitoramento;
using MassTransit;
using SerilogContext = Serilog.Context.LogContext;

namespace Infrastructure.Messaging;

/// <summary>
/// Consumer que recebe resultado da redução de estoque e executa confirmação ou compensação conforme necessário.
/// Implementa o lado "resultado" da saga coreografada de aprovação de orçamento.
/// </summary>
public class ReducaoEstoqueResultadoConsumer : IConsumer<ReducaoEstoqueResultado>
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IAppLogger _logger;
    private readonly IMetricsService _metrics;

    public ReducaoEstoqueResultadoConsumer(
        IOrdemServicoGateway gateway,
        IAppLogger logger,
        IMetricsService metrics)
    {
        _gateway = gateway;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task Consume(ConsumeContext<ReducaoEstoqueResultado> context)
    {
        var msg = context.Message;

        // Enriquecer todos os logs do consumer com o CorrelationId da mensagem
        using (SerilogContext.PushProperty("CorrelationId", msg.CorrelationId))
        {
            await ProcessarMensagemAsync(msg);
        }
    }

    private async Task ProcessarMensagemAsync(ReducaoEstoqueResultado msg)
    {
        var os = await _gateway.ObterPorIdAsync(msg.OrdemServicoId);
        if (os == null)
        {
            _logger.LogCritical(
                "OS {OsId} não encontrada ao processar resultado de estoque! CorrelationId: {CorrelationId}",
                msg.OrdemServicoId, msg.CorrelationId);
            return;
        }

        if (msg.Sucesso)
        {
            // Sempre confirma, independente do status atual da OS. Caso a OS passe por um retry, já estará confirmado.
            os.ConfirmarReducaoEstoque();
            await _gateway.AtualizarAsync(os);

            _logger.LogInformation(
                "Redução de estoque confirmada para OS {OsId}. Status atual: {Status}. CorrelationId: {CorrelationId}",
                os.Id, os.Status.Valor, msg.CorrelationId);

            _metrics.RegistrarEstoqueConfirmado(os.Id, os.Status.Valor.ToString(), msg.CorrelationId);

            return;
        }

        try
        {
            os.CompensarFalhaSaga();
            await _gateway.AtualizarAsync(os);

            _metrics.RegistrarCompensacaoSagaFalhaEstoque(os.Id, msg.MotivoFalha ?? "desconhecido", msg.CorrelationId);

            _logger.LogWarning(
                "Falha na redução de estoque para OS {OsId}. Motivo: {Motivo}. Status atual: {Status}. CorrelationId: {CorrelationId}. Compensação aplicada para OS {OsId}. Status revertido para Aprovada. CorrelationId: {CorrelationId}",
                os.Id, msg.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                "FALHA NA COMPENSAÇÃO para OS {OsId}! Dados possivelmente INCONSISTENTES. CorrelationId: {CorrelationId}",
                os.Id, msg.CorrelationId);

            _metrics.RegistrarCompensacaoSagaFalhaCritica(os.Id, ex.Message, msg.CorrelationId);
        }
    }
}
