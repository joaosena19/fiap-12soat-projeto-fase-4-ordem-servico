using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Application.Contracts.Monitoramento;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace Infrastructure.ExternalServices.Cadastro;

/// <summary>
/// Implementação HTTP dos serviços externos de Cadastro.
/// Faz chamadas REST ao microsserviço de Cadastros para obter dados de Clientes, Veículos e Serviços.
/// </summary>
public class CadastroHttpClientService : BaseExternalHttpClient,
    IClienteExternalService, IServicoExternalService, IVeiculoExternalService
{
    public CadastroHttpClientService(
        HttpClient httpClient, 
        IHttpContextAccessor httpContextAccessor,
        ICorrelationIdAccessor correlationIdAccessor)
        : base(httpClient, httpContextAccessor, correlationIdAccessor)
    {
    }

    #region IServicoExternalService

    /// <summary>
    /// Obtém um serviço por ID do microsserviço de Cadastros.
    /// </summary>
    public async Task<ServicoExternalDto?> ObterServicoPorIdAsync(Guid servicoId)
    {
        PropagateHeaders();
        
        var response = await _httpClient.GetAsync($"/api/cadastros/servicos/{servicoId}");
        
        if (!response.IsSuccessStatusCode)
            return null;
        
        var result = await response.Content.ReadFromJsonAsync<ServicoExternalDto>();
        return result;
    }

    #endregion

    #region IVeiculoExternalService

    /// <summary>
    /// Verifica se um veículo existe no microsserviço de Cadastros.
    /// </summary>
    public async Task<bool> VerificarExistenciaVeiculo(Guid veiculoId)
    {
        PropagateHeaders();
        
        var response = await _httpClient.GetAsync($"/api/cadastros/veiculos/{veiculoId}");
        
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Obtém um veículo por ID do microsserviço de Cadastros.
    /// </summary>
    public async Task<VeiculoExternalDto?> ObterVeiculoPorIdAsync(Guid veiculoId)
    {
        PropagateHeaders();
        
        var response = await _httpClient.GetAsync($"/api/cadastros/veiculos/{veiculoId}");
        
        if (!response.IsSuccessStatusCode)
            return null;
        
        var result = await response.Content.ReadFromJsonAsync<VeiculoExternalDto>();
        return result;
    }

    /// <summary>
    /// Obtém um veículo por placa do microsserviço de Cadastros.
    /// </summary>
    public async Task<VeiculoExternalDto?> ObterVeiculoPorPlacaAsync(string placa)
    {
        PropagateHeaders();
        
        var response = await _httpClient.GetAsync($"/api/cadastros/veiculos/placa/{Uri.EscapeDataString(placa)}");
        
        if (!response.IsSuccessStatusCode)
            return null;
        
        var result = await response.Content.ReadFromJsonAsync<VeiculoExternalDto>();
        return result;
    }

    /// <summary>
    /// Cria um novo veículo no microsserviço de Cadastros.
    /// </summary>
    public async Task<VeiculoExternalDto> CriarVeiculoAsync(CriarVeiculoExternalDto dto)
    {
        PropagateHeaders();
        
        var response = await _httpClient.PostAsJsonAsync("/api/cadastros/veiculos", dto);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<VeiculoExternalDto>();
        return result ?? throw new InvalidOperationException("Falha ao criar veículo: resposta vazia do serviço.");
    }

    #endregion

    #region IClienteExternalService

    /// <summary>
    /// Obtém o cliente dono de um veículo.
    /// Primeiro busca o veículo para obter o ClienteId, depois busca o cliente.
    /// </summary>
    public async Task<ClienteExternalDto?> ObterClientePorVeiculoIdAsync(Guid veiculoId)
    {
        PropagateHeaders();
        
        // Primeiro busca o veículo para obter o ClienteId
        var veiculo = await ObterVeiculoPorIdAsync(veiculoId);
        if (veiculo == null)
            return null;
        
        // Depois busca o cliente
        var clienteResponse = await _httpClient.GetAsync($"/api/cadastros/clientes/{veiculo.ClienteId}");
        
        if (!clienteResponse.IsSuccessStatusCode)
            return null;
        
        var result = await clienteResponse.Content.ReadFromJsonAsync<ClienteExternalDto>();
        return result;
    }

    /// <summary>
    /// Obtém um cliente por documento (CPF ou CNPJ) do microsserviço de Cadastros.
    /// </summary>
    public async Task<ClienteExternalDto?> ObterPorDocumentoAsync(string documentoIdentificador)
    {
        PropagateHeaders();
        
        var response = await _httpClient.GetAsync($"/api/cadastros/clientes/documento/{Uri.EscapeDataString(documentoIdentificador)}");
        
        if (!response.IsSuccessStatusCode)
            return null;
        
        var result = await response.Content.ReadFromJsonAsync<ClienteExternalDto>();
        return result;
    }

    /// <summary>
    /// Cria um novo cliente no microsserviço de Cadastros.
    /// </summary>
    public async Task<ClienteExternalDto> CriarClienteAsync(CriarClienteExternalDto dto)
    {
        PropagateHeaders();
        
        var response = await _httpClient.PostAsJsonAsync("/api/cadastros/clientes", dto);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ClienteExternalDto>();
        return result ?? throw new InvalidOperationException("Falha ao criar cliente: resposta vazia do serviço.");
    }

    #endregion
}
