using Application.Contracts.Gateways;
using Infrastructure.Messaging.DTOs;
using Application.Contracts.Monitoramento;
using MassTransit;

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
        await ProcessarMensagemAsync(msg);
    }

    private async Task ProcessarMensagemAsync(ReducaoEstoqueResultado msg)
    {
        var log = _logger
            .ComPropriedade("Consumer", nameof(ReducaoEstoqueResultadoConsumer))
            .ComPropriedade("CorrelationId", msg.CorrelationId)
            .ComPropriedade("OsId", msg.OrdemServicoId);
        
        var os = await _gateway.ObterPorIdAsync(msg.OrdemServicoId);
        if (os == null)
        {
            log.LogError(
                "Ordem Serviço não encontrada ao processar resultado de estoque.");
            return;
        }

        if (msg.Sucesso)
        {
            // Sempre confirma, independente do status atual da OS. Caso a OS passe por um retry, já estará confirmado.
            os.ConfirmarReducaoEstoque();
            await _gateway.AtualizarAsync(os);

            log.LogInformation(
                "Redução de estoque confirmada para Ordem Serviço. Status atual: {Status}.",
                os.Status.Valor);

            _metrics.RegistrarEstoqueConfirmado(os.Id, os.Status.Valor.ToString(), msg.CorrelationId);

            return;
        }

        try
        {
            os.RegistrarFalhaReducaoEstoque();
            await _gateway.AtualizarAsync(os);

            _metrics.RegistrarCompensacaoSagaFalhaEstoque(os.Id, msg.MotivoFalha ?? "desconhecido", msg.CorrelationId);

            log.LogWarning(
                "Falha na redução de estoque para Ordem Serviço. Motivo: {Motivo}. Status atual: {Status}. Compensação aplicada, status revertido para Aprovada.",
                msg.MotivoFalha ?? "desconhecido", os.Status.Valor);
        }
        catch (Exception ex)
        {
            log.LogError(ex,
                "FALHA NA COMPENSAÇÃO para Ordem Serviço! Dados possivelmente INCONSISTENTES.");

            _metrics.RegistrarCompensacaoSagaFalhaCritica(os.Id, ex.Message, msg.CorrelationId);
        }
    }
}
