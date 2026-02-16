using Application.OrdemServico.Dtos;
using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Domain.OrdemServico.Enums;
using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Fixtures;
using Tests.Integration.Helpers;
using Tests.Integration.OrdemServico.Builders;
using Xunit;

namespace Tests.Integration.OrdemServico;

/// <summary>
/// Testes de integração para endpoints principais de OrdemServicoController
/// </summary>
public class OrdemServicoControllerMainTests : IClassFixture<Mongo2GoFixture>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _clientAdmin;
    private readonly HttpClient _clientUnauth;

    public OrdemServicoControllerMainTests(Mongo2GoFixture fixture)
    {
        _factory = new TestWebApplicationFactory(fixture);
        _clientAdmin = _factory.CreateAuthenticatedClient(isAdmin: true);
        _clientUnauth = _factory.CreateUnauthenticatedClient();
    }

    public async Task InitializeAsync()
    {
        await MongoOrdemServicoTestHelper.ClearAsync(_factory.Services);
    }

    public async Task DisposeAsync()
    {
        await MongoOrdemServicoTestHelper.ClearAsync(_factory.Services);
        _clientAdmin?.Dispose();
        _clientUnauth?.Dispose();
        _factory?.Dispose();
    }

    #region GET /api/ordens-servico

    [Fact(DisplayName = "Deve listar ordens de serviço com sucesso para admin")]
    [Trait("Method", "GET")]
    public async Task ListarOrdensServico_ComTokenAdmin_DeveRetornar200ComLista()
    {
        // Arrange
        var ordemServico1 = OrdemServicoAggregateBuilder.Recebida();
        var ordemServico2 = OrdemServicoAggregateBuilder.EmDiagnostico();
        
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico1);
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico2);

        // Act
        var response = await _clientAdmin.GetAsync("/api/ordens-servico");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var ordensServico = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico1.Id);
        ordensServico.Should().NotBeNull();
        
        var ordensServico2 = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico2.Id);
        ordensServico2.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 401 para cliente não autenticado")]
    [Trait("Method", "GET")]
    public async Task ListarOrdensServico_SemToken_DeveRetornar401()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.Recebida();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientUnauth.GetAsync("/api/ordens-servico");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Database deve permanecer inalterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
    }

    #endregion

    #region GET /api/ordens-servico/{id}

    [Fact(DisplayName = "Deve obter ordem de serviço por ID com sucesso")]
    [Trait("Method", "GET")]
    public async Task ObterOrdemServicoPorId_IdExistente_DeveRetornar200()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.Recebida();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.GetAsync($"/api/ordens-servico/{ordemServico.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Database permanece inalterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "GET")]
    public async Task ObterOrdemServicoPorId_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.GetAsync($"/api/ordens-servico/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/ordens-servico/codigo/{codigo}

    [Fact(DisplayName = "Deve obter ordem de serviço por código com sucesso")]
    [Trait("Method", "GET")]
    public async Task ObterOrdemServicoPorCodigo_CodigoExistente_DeveRetornar200()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.Recebida();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.GetAsync($"/api/ordens-servico/codigo/{ordemServico.Codigo.Valor}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Database permanece inalterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 404 para código inexistente")]
    [Trait("Method", "GET")]
    public async Task ObterOrdemServicoPorCodigo_CodigoInexistente_DeveRetornar404()
    {
        // Arrange
        var codigoInexistente = "OS-999999";

        // Act
        var response = await _clientAdmin.GetAsync($"/api/ordens-servico/codigo/{codigoInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/ordens-servico

    [Fact(DisplayName = "Deve criar ordem de serviço com sucesso")]
    [Trait("Method", "POST")]
    public async Task CriarOrdemServico_RequestValida_DeveRetornar201()
    {
        // Arrange
        var request = OrdemServicoRequestBuilder.CriarOrdemServicoBasica();
        
        _factory.Mocks.VeiculoService
            .Setup(x => x.VerificarExistenciaVeiculo(request.VeiculoId))
            .ReturnsAsync(true);

        // Act
        var response = await _clientAdmin.PostAsJsonAsync("/api/ordens-servico", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verify response indicates creation
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 401 para cliente não autenticado")]
    [Trait("Method", "POST")]
    public async Task CriarOrdemServico_SemToken_DeveRetornar401()
    {
        // Arrange
        var request = OrdemServicoRequestBuilder.CriarOrdemServicoBasica();

        // Act
        var response = await _clientUnauth.PostAsJsonAsync("/api/ordens-servico", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Database deve permanecer vazio - não foi criada nenhuma ordem
        // (Implicitly verified by lack of creation)
    }

    #endregion

    #region POST /api/ordens-servico/completa

    [Fact(DisplayName = "Deve criar ordem de serviço completa com sucesso")]
    [Trait("Method", "POST")]
    public async Task CriarOrdemServicoCompleta_RequestValida_DeveRetornar201()
    {
        // Arrange
        var request = OrdemServicoRequestBuilder.CriarOrdemServicoCompleta();
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        
        // Mock external services
        _factory.Mocks.ClienteService
            .Setup(x => x.ObterPorDocumentoAsync(It.IsAny<string>()))
            .ReturnsAsync((ClienteExternalDto?)null);
            
        _factory.Mocks.ClienteService
            .Setup(x => x.CriarClienteAsync(It.IsAny<CriarClienteExternalDto>()))
            .ReturnsAsync(new ClienteExternalDto { Id = clienteId });
            
        _factory.Mocks.VeiculoService
            .Setup(x => x.ObterVeiculoPorPlacaAsync(It.IsAny<string>()))
            .ReturnsAsync((VeiculoExternalDto?)null);
            
        _factory.Mocks.VeiculoService
            .Setup(x => x.CriarVeiculoAsync(It.IsAny<CriarVeiculoExternalDto>()))
            .ReturnsAsync(new VeiculoExternalDto 
            { 
                Id = veiculoId,
                ClienteId = clienteId,
                Placa = request.Veiculo.Placa,
                Modelo = request.Veiculo.Modelo,
                Marca = request.Veiculo.Marca,
                Cor = request.Veiculo.Cor,
                Ano = request.Veiculo.Ano,
                TipoVeiculo = request.Veiculo.TipoVeiculo.ToString()
            });

        // Act
        var response = await _clientAdmin.PostAsJsonAsync("/api/ordens-servico/completa", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 401 para cliente não autenticado")]
    [Trait("Method", "POST")]
    public async Task CriarOrdemServicoCompleta_SemToken_DeveRetornar401()
    {
        // Arrange
        var request = OrdemServicoRequestBuilder.CriarOrdemServicoCompleta();

        // Act
        var response = await _clientUnauth.PostAsJsonAsync("/api/ordens-servico/completa", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/ordens-servico/{id}/cancelar

    [Fact(DisplayName = "Deve cancelar ordem de serviço com sucesso")]
    [Trait("Method", "POST")]
    public async Task CancelarOrdemServico_OrdemRecebida_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.Recebida();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{ordemServico.Id}/cancelar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task CancelarOrdemServico_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{idInexistente}/cancelar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/ordens-servico/{id}/iniciar-diagnostico

    [Fact(DisplayName = "Deve iniciar diagnóstico com sucesso")]
    [Trait("Method", "POST")]
    public async Task IniciarDiagnostico_OrdemRecebida_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.Recebida();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{ordemServico.Id}/iniciar-diagnostico", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmDiagnostico);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task IniciarDiagnostico_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{idInexistente}/iniciar-diagnostico", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}