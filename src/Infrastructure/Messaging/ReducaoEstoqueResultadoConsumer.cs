using Application.Contracts.Gateways;
using Application.Contracts.Messaging.DTOs;
using Application.Contracts.Monitoramento;
using Application.Extensions;
using Application.Extensions.Enums;
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
        var mensagem = context.Message;

        var logger = _logger
            .ComMensageria(NomeMensagemEnum.ReducaoEstoqueResultado, TipoMensagemEnum.Consumo)
            .ComPropriedade("Consumer", nameof(ReducaoEstoqueResultadoConsumer))
            .ComPropriedade("CorrelationId", mensagem.CorrelationId)
            .ComPropriedade("OrdemServicoId", mensagem.OrdemServicoId);

        try
        {
            await ProcessarMensagemAsync(mensagem, logger);
        }
        catch (Exception ex) 
        { 
            logger.LogError(ex, "Erro inesperado ao processar resultado de redução de estoque para Ordem Serviço.");
        }
    }

    private async Task ProcessarMensagemAsync(ReducaoEstoqueResultado mensagem, IAppLogger logger)
    {
        var ordemServico = await _gateway.ObterPorIdAsync(mensagem.OrdemServicoId);

        var deveProcessarMensagem = VerificarDeveProcessarMensagem(logger, ordemServico);
        if (!deveProcessarMensagem)
            return;

        if (mensagem.Sucesso)
        {
            await ProcessarMensagemSucesso(mensagem, logger, ordemServico!);
            return;
        }

        else if (!mensagem.Sucesso)
        {
            await ProcessarMensagemFalha(mensagem, logger, ordemServico!);
            return;
        }
    }

    private async Task ProcessarMensagemFalha(ReducaoEstoqueResultado mensagem, IAppLogger log, Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico ordemServico)
    {
        try
        {
            ordemServico!.RegistrarFalhaReducaoEstoque();
            await _gateway.AtualizarAsync(ordemServico);

            _metrics.RegistrarCompensacaoSagaFalhaEstoque(ordemServico.Id, mensagem.MotivoFalha ?? "desconhecido", mensagem.CorrelationId);

            log.LogWarning("Falha na redução de estoque para Ordem Serviço. Motivo: {Motivo}. Status atual: {Status}. Compensação aplicada, status revertido para Aprovada.", mensagem.MotivoFalha ?? "desconhecido", ordemServico.Status.Valor);
        }
        catch(Exception ex)
        {
            log.LogCritical(ex, "FALHA NA COMPENSAÇÃO para Ordem Serviço! Dados possivelmente INCONSISTENTES.");
            _metrics.RegistrarCompensacaoSagaFalhaCritica(mensagem.OrdemServicoId, ex.Message, mensagem.CorrelationId);
        }
        
    }

    private async Task ProcessarMensagemSucesso(ReducaoEstoqueResultado mensagem, IAppLogger log, Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico ordemServico)
    {
        try
        {
            // Sempre confirma, independente do status atual da OS. Caso a OS passe por um retry, já estará confirmado.
            ordemServico!.ConfirmarReducaoEstoque();
            await _gateway.AtualizarAsync(ordemServico);

            log.LogInformation("Redução de estoque confirmada para Ordem Serviço. Status atual: {Status}.", ordemServico.Status.Valor);

            _metrics.RegistrarEstoqueConfirmado(ordemServico.Id, ordemServico.Status.Valor.ToString(), mensagem.CorrelationId);
        }
        catch (Exception ex)
        {
            log.LogCritical(ex, "Falha ao confirmar redução de estoque para Ordem Serviço, mesmo após mensagem de sucesso do serviço de Estoque. Dados possivelmente inconsistentes. Detalhes do erro: {Mensagem}", ex.Message);
            _metrics.RegistrarCompensacaoSagaFalhaCritica(ordemServico.Id, ex.Message, mensagem.CorrelationId);
        }
    }

    /// <summary>
    /// Salvaguardas para processar a mensagem apenas quando fizer sentido, evitando alterações indevidas ou processamento duplicado. Retorna true se a mensagem deve ser processada, ou false se deve ser ignorada. Logs de erro são gerados para casos onde a mensagem é ignorada por inconsistência ou estado da OS. Logs de informação são gerados para casos onde a mensagem é ignorada por já ter sido processada com sucesso anteriormente.
    /// </summary>
    /// <returns></returns>
    private static bool VerificarDeveProcessarMensagem(IAppLogger log, Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico? ordemServico)
    {
        if (ordemServico is null)
        {
            log.LogError("Ordem Serviço não encontrada ao processar resultado de estoque.");
            return false;
        }

        if (!ordemServico.InteracaoEstoque.DeveRemoverEstoque)
        {
            log.LogError("Ordem Serviço não está configurada para redução de estoque (InteracaoEstoque.DeveRemoverEstoque = false), mas recebeu resultado de redução. Provável inconsistência dos dados. Mensagem ignorada e nenhuma alteração em banco feita.");
            return false;
        }

        if (ordemServico.InteracaoEstoque.DeveRemoverEstoque && ordemServico.InteracaoEstoque.EstoqueRemovidoComSucesso.HasValue && ordemServico.InteracaoEstoque.EstoqueRemovidoComSucesso.Value)
        {
            log.LogInformation("Ordem Serviço já teve estoque removido com sucesso. Ignorando resultado de redução de estoque para evitar processamento duplicado.");
            return false;
        }

        return true;
    }
}
