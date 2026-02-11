using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Application.Contracts.Monitoramento;
using Infrastructure.ExternalServices;
using System.Net.Http.Json;

namespace Infrastructure.ExternalServices.Estoque;

/// <summary>
/// Implementação HTTP do serviço externo de Estoque.
/// Faz chamadas REST ao microsserviço de Estoque para obter dados de itens de estoque.
/// Nota: A escrita no estoque (redução de quantidade) NÃO é feita via REST. 
/// É feita via mensageria (SQS + MassTransit) conforme implementado nas fases E/F do plano.
/// Headers são propagados automaticamente pelo PropagateHeadersHandler.
/// Erros HTTP são tratados de forma padronizada (5xx → 502, 4xx → status original).
/// </summary>
public class EstoqueHttpClientService : IEstoqueExternalService
{
    private readonly HttpClient _httpClient;
    private readonly IAppLogger _logger;

    public EstoqueHttpClientService(HttpClient httpClient, IAppLogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtém um item de estoque por ID do microsserviço de Estoque.
    /// </summary>
    /// <param name="itemId">ID do item de estoque</param>
    /// <returns>DTO do item de estoque ou null se não encontrado</returns>
    public async Task<ItemEstoqueExternalDto?> ObterItemEstoquePorIdAsync(Guid itemId)
    {
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync($"/api/estoque/itens/{itemId}"),
            nameof(ObterItemEstoquePorIdAsync),
            _logger);
            
        // 404 é esperado para itens não encontrados - retornar null
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(ObterItemEstoquePorIdAsync), _logger);
        
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
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync(
                $"/api/estoque/itens/{itemId}/disponibilidade?quantidadeRequisitada={quantidadeNecessaria}"),
            nameof(VerificarDisponibilidadeAsync),
            _logger);
            
        // 404 significa que o item não existe - retornar false (não disponível)
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(VerificarDisponibilidadeAsync), _logger);
        
        var result = await response.Content.ReadFromJsonAsync<DisponibilidadeExternalDto>();
        return result?.Disponivel ?? false;
    }


}
