using Application.Contracts.Gateways;
using Application.Contracts.Presenters;
using Application.Identidade.Services;
using Application.Identidade.Services.Extensions;
using Application.OrdemServico.Dtos;
using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Domain.OrdemServico.Enums;
using Shared.Enums;
using Shared.Exceptions;
using Application.Extensions;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;
using Application.Contracts.Monitoramento;

namespace Application.OrdemServico.UseCases;

/// <summary>
/// Cria uma ordem de serviço completa, incluindo cliente, veículo, serviços e itens de estoque. Tenta buscar clientes e veículos existentes antes de criar novos. Caso serviços ou itens não sejam encontrados, são ignorados.
/// </summary>
public class CriarOrdemServicoCompletaUseCase
{
    public async Task ExecutarAsync(
        Ator ator,
        CriarOrdemServicoCompletaDto dto,
        IOrdemServicoGateway ordemServicoGateway,
        IClienteExternalService clienteExternalService,
        IVeiculoExternalService veiculoExternalService,
        IServicoExternalService servicoExternalService,
        IEstoqueExternalService estoqueExternalService,
        ICriarOrdemServicoCompletaPresenter presenter,
        IAppLogger logger,
        IMetricsService metricsService)
    {
        try
        {
            if (!ator.PodeGerenciarOrdemServico())
                throw new DomainException("Acesso negado. Apenas administradores podem criar ordens de serviço completas.", ErrorType.NotAllowed, "Acesso negado para criar ordem de serviço completa para usuário {Ator_UsuarioId}", ator.UsuarioId);

            var cliente = await BuscarOuCriarCliente(dto.Cliente, clienteExternalService);
            var veiculo = await BuscarOuCriarVeiculo(dto.Veiculo, cliente.Id, veiculoExternalService);
            var novaOrdemServico = await CriarOrdemServicoComCodigoUnico(veiculo.Id, ordemServicoGateway);

            await AdicionarServicos(dto.ServicosIds, novaOrdemServico, servicoExternalService);
            await AdicionarItens(dto.Itens, novaOrdemServico, estoqueExternalService);

            var result = await ordemServicoGateway.SalvarAsync(novaOrdemServico);

            RegistrarMetricaOrdemServicoCriada(result.Id, cliente.Id, ator, metricsService, logger);

            presenter.ApresentarSucesso(result);
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

    private async Task<ClienteExternalDto> BuscarOuCriarCliente(ClienteDto clienteDto, IClienteExternalService clienteExternalService)
    {
        var clienteExistente = await clienteExternalService.ObterPorDocumentoAsync(clienteDto.DocumentoIdentificador);
        if (clienteExistente != null) return clienteExistente;

        var novoCliente = await clienteExternalService.CriarClienteAsync(new CriarClienteExternalDto
        {
            Nome = clienteDto.Nome,
            DocumentoIdentificador = clienteDto.DocumentoIdentificador
        });
        
        return novoCliente;
    }

    private async Task<VeiculoExternalDto> BuscarOuCriarVeiculo(VeiculoDto veiculoDto, Guid clienteId, IVeiculoExternalService veiculoExternalService)
    {
        var veiculoExistente = await veiculoExternalService.ObterVeiculoPorPlacaAsync(veiculoDto.Placa);
        if (veiculoExistente != null) return veiculoExistente;

        var novoVeiculo = await veiculoExternalService.CriarVeiculoAsync(new CriarVeiculoExternalDto
        {
            ClienteId = clienteId,
            Placa = veiculoDto.Placa,
            Modelo = veiculoDto.Modelo,
            Marca = veiculoDto.Marca,
            Cor = veiculoDto.Cor,
            Ano = veiculoDto.Ano,
            TipoVeiculo = (int)veiculoDto.TipoVeiculo
        });

        return novoVeiculo;
    }

    private async Task<OrdemServicoAggregate> CriarOrdemServicoComCodigoUnico(Guid veiculoId, IOrdemServicoGateway ordemServicoGateway)
    {
        OrdemServicoAggregate novaOrdemServico;
        OrdemServicoAggregate? ordemServicoExistente;

        do
        {
            novaOrdemServico = OrdemServicoAggregate.Criar(veiculoId);
            ordemServicoExistente = await ordemServicoGateway.ObterPorCodigoAsync(novaOrdemServico.Codigo.Valor);
        } while (ordemServicoExistente != null);

        return novaOrdemServico;
    }

    /// <summary>
    /// Adiciona serviços. Caso não encontre o serviço pelo ID, apenas ignora.
    /// </summary>
    private async Task AdicionarServicos(List<Guid>? servicosIds, OrdemServicoAggregate ordemServico, IServicoExternalService servicoExternalService)
    {
        if (servicosIds == null || servicosIds.Count == 0) return;

        foreach (var servicoId in servicosIds)
        {
            var servico = await servicoExternalService.ObterServicoPorIdAsync(servicoId);
            if (servico == null) continue;
            
            ordemServico.AdicionarServico(servico.Id, servico.Nome, servico.Preco);
        }
    }

    /// <summary>
    /// Adiciona itens. Caso não encontre o item pelo ID, apenas ignora.
    /// </summary>
    private async Task AdicionarItens(List<ItemDto>? itens, OrdemServicoAggregate ordemServico, IEstoqueExternalService estoqueExternalService)
    {
        if (itens == null || itens.Count == 0) return;

        foreach (var itemDto in itens)
        {
            var itemEstoque = await estoqueExternalService.ObterItemEstoquePorIdAsync(itemDto.ItemEstoqueId);
            if (itemEstoque == null) continue;

            ordemServico.AdicionarItem(
                itemEstoque.Id,
                itemEstoque.Nome,
                itemEstoque.Preco,
                itemDto.Quantidade,
                itemEstoque.TipoItemIncluido);
        }
    }

    /// <summary>
    /// Registra métricas para uma ordem de serviço criada. Não lança exception em caso de falha.
    /// </summary>
    private void RegistrarMetricaOrdemServicoCriada(Guid ordemServicoId, Guid clienteId, Ator ator, IMetricsService metricsService, IAppLogger logger)
    {
        try
        {
            metricsService.RegistrarOrdemServicoCriada(ordemServicoId, clienteId, ator.UsuarioId);
        }
        catch (Exception ex)
        {
            logger.ComUseCase(this)
                  .ComAtor(ator)
                  .LogError(ex, "Erro ao registrar métrica de ordem de serviço criada. OrdemServicoId: {OrdemServicoId}", ordemServicoId);
        }
    }


}