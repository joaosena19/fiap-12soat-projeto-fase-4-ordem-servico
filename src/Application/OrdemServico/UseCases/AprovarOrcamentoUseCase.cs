using Application.Contracts.Gateways;
using Application.Contracts.Presenters;
using Application.Contracts.Messaging;
using Application.Contracts.Messaging.DTOs;
using Application.Identidade.Services;
using Application.Identidade.Services.Extensions;
using Application.OrdemServico.Interfaces.External;
using Domain.OrdemServico.Enums;
using Shared.Enums;
using Shared.Exceptions;
using Application.Extensions;
using Application.Contracts.Monitoramento;

namespace Application.OrdemServico.UseCases;

public class AprovarOrcamentoUseCase
{
    public async Task ExecutarAsync(Ator ator, Guid ordemServicoId, IOrdemServicoGateway gateway, IVeiculoExternalService veiculoExternalService, IEstoqueMessagePublisher estoqueMessagePublisher, ICorrelationIdAccessor correlationIdAccessor, IOperacaoOrdemServicoPresenter presenter, IAppLogger logger)
    {
        try
        {
            var ordemServico = await gateway.ObterPorIdAsync(ordemServicoId) 
                ?? throw new DomainException("Ordem de serviço não encontrada.", ErrorType.ResourceNotFound, "Ordem de serviço não encontrada para Id {OrdemServicoId}", ordemServicoId);

            if (!await ator.PodeAprovarDesaprovarOrcamento(ordemServico, veiculoExternalService))
                throw new DomainException("Acesso negado. Apenas administradores ou donos da ordem de serviço podem aprovar orçamentos.", ErrorType.NotAllowed, "Acesso negado para aprovar orçamento da ordem de serviço {OrdemServicoId} para usuário {Ator_UsuarioId}", ordemServicoId, ator.UsuarioId);

            if (ordemServico.Status.Valor == StatusOrdemServicoEnum.AguardandoAprovacao)
                ordemServico.AprovarOrcamento();
            else if (ordemServico.Status.Valor != StatusOrdemServicoEnum.Aprovada)
                throw new DomainException($"Só é possível aprovar orçamento para uma OS com status '{StatusOrdemServicoEnum.AguardandoAprovacao}' ou '{StatusOrdemServicoEnum.Aprovada}'.", ErrorType.DomainRuleBroken, "Tentativa de aprovar OS {OrdemServicoId} com status inválido {Status}", ordemServicoId, ordemServico.Status.Valor);
            // Se já está Aprovada (retry), pula AprovarOrcamento() e vai direto para IniciarExecucao()

            ordemServico.IniciarExecucao();

            // Enviar mensagem SQS apenas se:
            // - OS tem itens de estoque (DeveRemoverEstoque == true)
            // - Estoque ainda não foi confirmado de uma tentativa anterior (EstoqueRemovidoComSucesso != true)
            if (ordemServico.InteracaoEstoque.DeveRemoverEstoque && ordemServico.InteracaoEstoque.EstoqueRemovidoComSucesso != true)
            {
                var solicitacao = new ReducaoEstoqueSolicitacao
                {
                    CorrelationId = correlationIdAccessor.GetCorrelationId(),
                    OrdemServicoId = ordemServico.Id,
                    Itens = ordemServico.ItensIncluidos.Select(i => new ItemReducao
                    {
                        ItemEstoqueId = i.ItemEstoqueOriginalId,
                        Quantidade = i.Quantidade.Valor
                    }).ToList()
                };

                await estoqueMessagePublisher.PublicarSolicitacaoReducaoAsync(solicitacao);

                logger.LogInformation(
                    "Mensagem de redução de estoque publicada para OS {OsId}. CorrelationId: {CorrelationId}. Itens: {QtdItens}",
                    ordemServico.Id, solicitacao.CorrelationId, solicitacao.Itens.Count);
            }
            else
            {
                logger.LogInformation(
                    "OS {OsId} não requer interação com estoque (DeveRemover={Deve}, JaConfirmado={Conf}). Prosseguindo direto.",
                    ordemServico.Id, ordemServico.InteracaoEstoque.DeveRemoverEstoque,
                    ordemServico.InteracaoEstoque?.EstoqueRemovidoComSucesso ?? false);
            }

            await gateway.AtualizarAsync(ordemServico);
            presenter.ApresentarSucesso();
        }
        catch (DomainException ex)
        {
            logger.ComUseCase(this)
                  .ComAtor(ator)
                  .ComDomainErrorType(ex)
                  .LogInformation(ex.LogTemplate, ex.LogArgs);

            presenter.ApresentarErro(ex.Message, ex.ErrorType);
        }
        catch (Exception ex)
        {
            logger.ComUseCase(this)
                  .ComAtor(ator)
                  .LogError(ex, "Erro interno do servidor.");

            presenter.ApresentarErro("Erro interno do servidor.", ErrorType.UnexpectedError);
        }
    }
}