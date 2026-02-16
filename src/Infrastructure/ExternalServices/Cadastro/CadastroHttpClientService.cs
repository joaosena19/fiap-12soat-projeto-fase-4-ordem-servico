using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Application.Contracts.Monitoramento;
using Infrastructure.ExternalServices;
using System.Net.Http.Json;

namespace Infrastructure.ExternalServices.Cadastro;

/// <summary>
/// Implementação HTTP dos serviços externos de Cadastro.
/// Faz chamadas REST ao microsserviço de Cadastros para obter dados de Clientes, Veículos e Serviços.
/// Headers são propagados automaticamente pelo PropagateHeadersHandler.
/// Erros HTTP são tratados de forma padronizada (5xx → 502, 4xx → status original).
/// </summary>
public class CadastroHttpClientService : 
    IClienteExternalService, IServicoExternalService, IVeiculoExternalService
{
    private readonly HttpClient _httpClient;
    private readonly IAppLogger _logger;

    public CadastroHttpClientService(HttpClient httpClient, IAppLogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IServicoExternalService

    /// <summary>
    /// Obtém um serviço por ID do microsserviço de Cadastros.
    /// </summary>
    public async Task<ServicoExternalDto?> ObterServicoPorIdAsync(Guid servicoId)
    {
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync($"/api/cadastros/servicos/{servicoId}"),
            nameof(ObterServicoPorIdAsync),
            _logger);
            
        // 404 é esperado para recursos não encontrados - retornar null
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(ObterServicoPorIdAsync), _logger);
        
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
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync($"/api/cadastros/veiculos/{veiculoId}"),
            nameof(VerificarExistenciaVeiculo),
            _logger);
            
        // 404 significa que o veículo não existe - retornar false
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(VerificarExistenciaVeiculo), _logger);
        
        return true;
    }

    /// <summary>
    /// Obtém um veículo por ID do microsserviço de Cadastros.
    /// </summary>
    public async Task<VeiculoExternalDto?> ObterVeiculoPorIdAsync(Guid veiculoId)
    {
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync($"/api/cadastros/veiculos/{veiculoId}"),
            nameof(ObterVeiculoPorIdAsync),
            _logger);
            
        // 404 é esperado para veículos não encontrados - retornar null
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(ObterVeiculoPorIdAsync), _logger);
        
        var result = await response.Content.ReadFromJsonAsync<VeiculoExternalDto>();
        return result;
    }

    /// <summary>
    /// Obtém um veículo por placa do microsserviço de Cadastros.
    /// </summary>
    public async Task<VeiculoExternalDto?> ObterVeiculoPorPlacaAsync(string placa)
    {
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync($"/api/cadastros/veiculos/placa/{Uri.EscapeDataString(placa)}"),
            nameof(ObterVeiculoPorPlacaAsync),
            _logger);
            
        // 404 é esperado para placas não encontradas - retornar null
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(ObterVeiculoPorPlacaAsync), _logger);
        
        var result = await response.Content.ReadFromJsonAsync<VeiculoExternalDto>();
        return result;
    }

    /// <summary>
    /// Cria um novo veículo no microsserviço de Cadastros.
    /// </summary>
    public async Task<VeiculoExternalDto> CriarVeiculoAsync(CriarVeiculoExternalDto dto)
    {
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.PostAsJsonAsync("/api/cadastros/veiculos", dto),
            nameof(CriarVeiculoAsync),
            _logger);
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(CriarVeiculoAsync), _logger);
        
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
        // Primeiro busca o veículo para obter o ClienteId
        var veiculo = await ObterVeiculoPorIdAsync(veiculoId);
        if (veiculo == null)
            return null;
        
        // Depois busca o cliente
        var clienteResponse = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync($"/api/cadastros/clientes/{veiculo.ClienteId}"),
            nameof(ObterClientePorVeiculoIdAsync),
            _logger);
            
        // 404 é esperado para clientes não encontrados - retornar null
        if (clienteResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(clienteResponse, nameof(ObterClientePorVeiculoIdAsync), _logger);
        
        var result = await clienteResponse.Content.ReadFromJsonAsync<ClienteExternalDto>();
        return result;
    }

    /// <summary>
    /// Obtém um cliente por documento (CPF ou CNPJ) do microsserviço de Cadastros.
    /// </summary>
    public async Task<ClienteExternalDto?> ObterPorDocumentoAsync(string documentoIdentificador)
    {
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.GetAsync($"/api/cadastros/clientes/documento/{Uri.EscapeDataString(documentoIdentificador)}"),
            nameof(ObterPorDocumentoAsync),
            _logger);
            
        // 404 é esperado para documentos não encontrados - retornar null
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(ObterPorDocumentoAsync), _logger);
        
        var result = await response.Content.ReadFromJsonAsync<ClienteExternalDto>();
        return result;
    }

    /// <summary>
    /// Cria um novo cliente no microsserviço de Cadastros.
    /// </summary>
    public async Task<ClienteExternalDto> CriarClienteAsync(CriarClienteExternalDto dto)
    {
        var response = await BaseExternalHttpClient.ExecuteHttpOperationAsync(
            () => _httpClient.PostAsJsonAsync("/api/cadastros/clientes", dto),
            nameof(CriarClienteAsync),
            _logger);
            
        await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, nameof(CriarClienteAsync), _logger);
        
        var result = await response.Content.ReadFromJsonAsync<ClienteExternalDto>();
        return result ?? throw new InvalidOperationException("Falha ao criar cliente: resposta vazia do serviço.");
    }

    #endregion
}
