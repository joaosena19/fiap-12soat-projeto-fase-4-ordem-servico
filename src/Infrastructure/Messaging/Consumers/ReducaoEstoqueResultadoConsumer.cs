using Application.Contracts.Gateways;
using Application.Contracts.Messaging;
using Application.Contracts.Monitoramento;
using MassTransit;

namespace Infrastructure.Messaging.Consumers;

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

        var os = await _gateway.ObterPorIdAsync(msg.OrdemServicoId);
        if (os == null)
        {
            _logger.LogError(
                "OS {OsId} não encontrada ao processar resultado de estoque! CorrelationId: {CorrelationId}",
                msg.OrdemServicoId, msg.CorrelationId);
            return;
        }

        if (msg.Sucesso)
        {
            // SEMPRE confirma, independente do status atual da OS.
            // Cobre race condition: BackgroundService pode ter compensado a OS para Aprovada,
            // mas o Estoque processou a redução a tempo. O flag EstoqueRemovidoComSucesso = true
            // garante que a re-tentativa (dual-path em F-01) NÃO envia nova mensagem SQS.
            os.ConfirmarReducaoEstoque();
            await _gateway.AtualizarAsync(os);

            _logger.LogInformation(
                "Redução de estoque confirmada para OS {OsId}. Status atual: {Status}. CorrelationId: {CorrelationId}",
                os.Id, os.Status.Valor, msg.CorrelationId);

            _metrics.RegistrarEstoqueConfirmado(os.Id, os.Status.Valor.ToString(), msg.CorrelationId);

            return;
        }

        // FALHA: Compensar apenas se a OS ainda está em EmExecucao
        _logger.LogWarning(
            "Falha na redução de estoque para OS {OsId}. Motivo: {Motivo}. Status atual: {Status}. CorrelationId: {CorrelationId}",
            os.Id, msg.MotivoFalha, os.Status.Valor, msg.CorrelationId);

        try
        {
            os.CompensarFalhaSaga(); // EmExecucao → Aprovada + InteracaoEstoque.MarcarFalha()
            await _gateway.AtualizarAsync(os);

            _metrics.RegistrarCompensacaoSagaFalhaEstoque(os.Id, msg.MotivoFalha ?? "desconhecido", msg.CorrelationId);

            _logger.LogWarning(
                "Compensação aplicada para OS {OsId}. Status revertido para Aprovada. CorrelationId: {CorrelationId}",
                os.Id, msg.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "FALHA NA COMPENSAÇÃO para OS {OsId}! Dados possivelmente INCONSISTENTES. CorrelationId: {CorrelationId}",
                os.Id, msg.CorrelationId);

            _metrics.RegistrarCompensacaoSagaFalhaCritica(os.Id, ex.Message, msg.CorrelationId);
        }
    }
}
