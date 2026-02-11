using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Application.Contracts.Monitoramento;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace Infrastructure.ExternalServices.Estoque;

/// <summary>
/// Implementação HTTP do serviço externo de Estoque.
/// Faz chamadas REST ao microsserviço de Estoque para obter dados de itens de estoque.
/// Nota: A escrita no estoque (redução de quantidade) NÃO é feita via REST. 
/// É feita via mensageria (SQS + MassTransit) conforme implementado nas fases E/F do plano.
/// </summary>
public class EstoqueHttpClientService : BaseExternalHttpClient, IEstoqueExternalService
{
    public EstoqueHttpClientService(
        HttpClient httpClient, 
        IHttpContextAccessor httpContextAccessor,
        ICorrelationIdAccessor correlationIdAccessor)
        : base(httpClient, httpContextAccessor, correlationIdAccessor)
    {
    }

    /// <summary>
    /// Obtém um item de estoque por ID do microsserviço de Estoque.
    /// </summary>
    /// <param name="itemId">ID do item de estoque</param>
    /// <returns>DTO do item de estoque ou null se não encontrado</returns>
    public async Task<ItemEstoqueExternalDto?> ObterItemEstoquePorIdAsync(Guid itemId)
    {
        PropagateHeaders();
        
        var response = await _httpClient.GetAsync($"/api/estoque/itens/{itemId}");
        
        if (!response.IsSuccessStatusCode)
            return null;
        
        var result = await response.Content.ReadFromJsonAsync<ItemEstoqueExternalDto>();
        return result;
    }

    /// <summary>
    /// Verifica a disponibilidade de um item de estoque no microsserviço de Estoque.
    /// </summary>
    /// <param name="itemId">ID do item de estoque</param>
    /// <param name="quantidadeNecessaria">Quantidade necessária</param>
    /// <returns>True se o item está disponível na quantidade solicitada, false caso contrário</returns>
    public async Task<bool> VerificarDisponibilidadeAsync(Guid itemId, int quantidadeNecessaria)
    {
        PropagateHeaders();
        
        var response = await _httpClient.GetAsync(
            $"/api/estoque/itens/{itemId}/disponibilidade?quantidadeRequisitada={quantidadeNecessaria}");
        
        if (!response.IsSuccessStatusCode)
            return false;
        
        var result = await response.Content.ReadFromJsonAsync<DisponibilidadeExternalDto>();
        return result?.Disponivel ?? false;
    }


}
