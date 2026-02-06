using Application.Contracts.Gateways;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Application.Identidade.Services.Extensions;

/// <summary>
/// Extensions do Ator para Ordem de Serviço.
/// Modificado para não depender de gateways de outros bounded contexts.
/// Verificações de autorização recebem o ClienteId já resolvido pelo use case.
/// </summary>
public static class AtorOrdemServicoExtensions
{
    /// <summary>
    /// Administrador ou dono do veículo
    /// </summary>
    /// <param name="ator">Ator tentando acessar</param>
    /// <param name="clienteIdDoVeiculo">Cliente Id do veículo associado à ordem de serviço (resolvido pelo use case)</param>
    public static bool PodeAcessarOrdemServico(this Ator ator, Guid? clienteIdDoVeiculo)
    {
        if (ator.PodeGerenciarSistema()) return true;
        return clienteIdDoVeiculo == ator.ClienteId;
    }

    /// <summary>
    /// Administrador ou dono do veículo
    /// </summary>
    public static bool PodeCriarOrdemServicoParaVeiculo(this Ator ator, Guid? clienteIdDoVeiculo)
    {
        if (ator.PodeGerenciarSistema()) return true;
        return clienteIdDoVeiculo == ator.ClienteId;
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
    /// <param name="clienteIdDoVeiculo">Cliente Id do veículo associado à ordem de serviço (resolvido pelo use case)</param>
    /// <returns>True se o ator pode aprovar/desaprovar orçamentos, False caso contrário</returns>
    public static bool PodeAprovarDesaprovarOrcamento(this Ator ator, Guid? clienteIdDoVeiculo)
    {
        // Administradores e sistema podem aprovar/desaprovar qualquer orçamento
        if (ator.PodeGerenciarSistema() || ator.PodeAcionarWebhooks()) return true;

        // Cliente pode aprovar/desaprovar orçamento apenas se for dono do veículo
        return clienteIdDoVeiculo == ator.ClienteId;
    }
}