using Application.Contracts.Gateways;
using Application.OrdemServico.Interfaces.External;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Application.Identidade.Services.Extensions;

/// <summary>
/// Extensions do Ator para Ordem de Serviço.
/// Usa serviços externos para acessar dados de outros bounded contexts (Cadastros).
/// </summary>
public static class AtorOrdemServicoExtensions
{
    /// <summary>
    /// Administrador ou dono do veículo
    /// </summary>
    /// <param name="ator">Ator tentando acessar</param>
    /// <param name="ordemServico">Ordem de serviço sendo acessada</param>
    /// <param name="veiculoExternalService">Serviço externo para acessar dados de veículos</param>
    public static async Task<bool> PodeAcessarOrdemServicoAsync(this Ator ator, OrdemServicoAggregate ordemServico, IVeiculoExternalService veiculoExternalService)
    {
        if (ator.PodeGerenciarSistema()) return true;

        var veiculo = await veiculoExternalService.ObterVeiculoPorIdAsync(ordemServico.VeiculoId);
        return veiculo?.ClienteId == ator.ClienteId;
    }

    /// <summary>
    /// Administrador ou dono do veículo
    /// </summary>
    /// <param name="ator">Ator tentando acessar</param>
    /// <param name="ordemServicoId">Id da ordem de serviço</param>
    /// <param name="ordemServicoGateway">Gateway para acessar dados da ordem de serviço</param>
    /// <param name="veiculoExternalService">Serviço externo para acessar dados de veículos</param>
    public static async Task<bool> PodeAcessarOrdemServicoAsync(this Ator ator, Guid ordemServicoId, IOrdemServicoGateway ordemServicoGateway, IVeiculoExternalService veiculoExternalService)
    {
        if (ator.PodeGerenciarSistema()) return true;

        var ordemServico = await ordemServicoGateway.ObterPorIdAsync(ordemServicoId);
        if (ordemServico == null) return false;

        return await ator.PodeAcessarOrdemServicoAsync(ordemServico, veiculoExternalService);
    }

    /// <summary>
    /// Administrador ou dono do veículo
    /// </summary>
    /// <param name="ator">Ator tentando criar ordem de serviço</param>
    /// <param name="veiculoId">Id do veículo</param>
    /// <param name="veiculoExternalService">Serviço externo para acessar dados de veículos</param>
    public static async Task<bool> PodeCriarOrdemServicoParaVeiculoAsync(this Ator ator, Guid veiculoId, IVeiculoExternalService veiculoExternalService)
    {
        if (ator.PodeGerenciarSistema()) return true;

        var veiculo = await veiculoExternalService.ObterVeiculoPorIdAsync(veiculoId);
        return veiculo != null && veiculo.ClienteId == ator.ClienteId;
    }

    /// <summary>
    /// Somente administrador ou sistema (webhooks)
    /// </summary>
    public static bool PodeAtualizarStatusOrdem(this Ator ator)
    {
        return ator.PodeGerenciarSistema() || ator.PodeAcionarWebhooks();
    }

    /// <summary>
    /// Verifica se o ator tem permissão para gerenciar ordens de serviço.
    /// Apenas administradores podem gerenciar ordens de serviço.
    /// </summary>
    /// <param name="ator">O ator que está tentando realizar a operação</param>
    /// <returns>True se o ator pode gerenciar ordens de serviço, False caso contrário</returns>
    public static bool PodeGerenciarOrdemServico(this Ator ator)
    {
        return ator.PodeGerenciarSistema();
    }

    /// <summary>
    /// Verifica se o ator pode aprovar/desaprovar orçamentos.
    /// Administradores, donos da ordem de serviço ou sistema (webhooks) podem aprovar/desaprovar orçamentos.
    /// </summary>
    /// <param name="ator">O ator que está tentando realizar a operação</param>
    /// <param name="ordemServico">A ordem de serviço relacionada ao orçamento</param>
    /// <param name="veiculoExternalService">Serviço externo para acessar dados de veículos</param>
    /// <returns>True se o ator pode aprovar/desaprovar orçamentos, False caso contrário</returns>
    public static async Task<bool> PodeAprovarDesaprovarOrcamento(this Ator ator, OrdemServicoAggregate ordemServico, IVeiculoExternalService veiculoExternalService)
    {
        // Administradores e sistema podem aprovar/desaprovar qualquer orçamento
        if (ator.PodeGerenciarSistema() || ator.PodeAcionarWebhooks()) return true;

        // Cliente pode aprovar/desaprovar orçamento apenas se for dono do veículo
        var veiculo = await veiculoExternalService.ObterVeiculoPorIdAsync(ordemServico.VeiculoId);
        return veiculo?.ClienteId == ator.ClienteId;
    }
}