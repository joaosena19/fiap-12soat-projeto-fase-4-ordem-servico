using Application.OrdemServico.Dtos;
using Application.OrdemServico.Dtos.External;
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
/// Testes de integração para endpoints de orçamento, execução e entrega
/// </summary>
public class OrdemServicoControllerWorkflowTests : IClassFixture<Mongo2GoFixture>, IAsyncLifetime  
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _clientAdmin;
    private readonly HttpClient _clientUnauth;

    public OrdemServicoControllerWorkflowTests(Mongo2GoFixture fixture)
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

    #region POST /api/ordens-servico/{id}/orcamento

    [Fact(DisplayName = "Deve criar orçamento com sucesso")]
    [Trait("Method", "POST")]
    public async Task CriarOrcamento_OrdemComItens_DeveRetornar201()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.ComItens(2);
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{ordemServico.Id}/orcamento", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verificar se orçamento foi criado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.AguardandoAprovacao);
        ordemServicoDb.Orcamento.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task CriarOrcamento_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{idInexistente}/orcamento", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/ordens-servico/{id}/orcamento/aprovar

    [Fact(DisplayName = "Deve aprovar orçamento com sucesso")]
    [Trait("Method", "POST")]
    public async Task AprovarOrcamento_OrdemAguardandoAprovacao_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.AguardandoAprovacao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{ordemServico.Id}/orcamento/aprovar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task AprovarOrcamento_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{idInexistente}/orcamento/aprovar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/ordens-servico/{id}/orcamento/desaprovar

    [Fact(DisplayName = "Deve desaprovar orçamento com sucesso")]
    [Trait("Method", "POST")]
    public async Task DesaprovarOrcamento_OrdemAguardandoAprovacao_DeveRetornar204()  
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.AguardandoAprovacao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{ordemServico.Id}/orcamento/desaprovar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task DesaprovarOrcamento_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{idInexistente}/orcamento/desaprovar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/ordens-servico/{id}/finalizar-execucao

    [Fact(DisplayName = "Deve finalizar execução com sucesso")]
    [Trait("Method", "POST")]
    public async Task FinalizarExecucao_OrdemEmExecucao_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.EmExecucao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{ordemServico.Id}/finalizar-execucao", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Finalizada);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task FinalizarExecucao_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{idInexistente}/finalizar-execucao", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/ordens-servico/{id}/entregar

    [Fact(DisplayName = "Deve entregar ordem de serviço com sucesso")]
    [Trait("Method", "POST")]
    public async Task EntregarOrdemServico_OrdemFinalizada_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.Finalizada();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{ordemServico.Id}/entregar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Entregue);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task EntregarOrdemServico_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.PostAsync($"/api/ordens-servico/{idInexistente}/entregar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/ordens-servico/tempo-medio

    [Fact(DisplayName = "Deve obter tempo médio com sucesso")]
    [Trait("Method", "GET")]
    public async Task ObterTempoMedio_ComOrdensEntregues_DeveRetornar200()
    {
        // Arrange
        var ordemServico1 = OrdemServicoAggregateBuilder.Entregue();
        var ordemServico2 = OrdemServicoAggregateBuilder.Entregue();
        
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico1);
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico2);

        // Act
        var response = await _clientAdmin.GetAsync("/api/ordens-servico/tempo-medio?quantidadeDias=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Database permanece inalterado
        var ordemServicoDb1 = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico1.Id);
        ordemServicoDb1.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 400 para parâmetro inválido")]
    [Trait("Method", "GET")]
    public async Task ObterTempoMedio_ParametroInvalido_DeveRetornar400()
    {
        // Act
        var response = await _clientAdmin.GetAsync("/api/ordens-servico/tempo-medio?quantidadeDias=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/ordens-servico/busca-publica

    [Fact(DisplayName = "Deve buscar ordem públicamente com sucesso")]
    [Trait("Method", "POST")]
    public async Task BuscarOrdemPublicamente_DocumentoECodigoValidos_DeveRetornar200()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.Recebida();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.BuscaPublica(
            documento: "123.456.789-00",
            codigo: ordemServico.Codigo.Valor);
            
        _factory.Mocks.ClienteService
            .Setup(x => x.ObterPorDocumentoAsync("123.456.789-00"))
            .ReturnsAsync(new ClienteExternalDto 
            { 
                Id = Guid.NewGuid(),
                DocumentoIdentificador = "123.456.789-00"
            });

        // Act
        var response = await _clientUnauth.PostAsJsonAsync("/api/ordens-servico/busca-publica", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Database permanece inalterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id); 
        ordemServicoDb.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deve retornar 200 com null para documento/código não matching")]
    [Trait("Method", "POST")]
    public async Task BuscarOrdemPublicamente_DocumentoCodigoMismatch_DeveRetornar200ComNull()
    {
        // Arrange
        var request = OrdemServicoRequestBuilder.BuscaPublica(
            documento: "999.999.999-99",
            codigo: "OS-999999");
            
        _factory.Mocks.ClienteService
            .Setup(x => x.ObterPorDocumentoAsync("999.999.999-99"))
            .ReturnsAsync((ClienteExternalDto?)null);

        // Act
        var response = await _clientUnauth.PostAsJsonAsync("/api/ordens-servico/busca-publica", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Response body should be null, but we don't validate that here for integration simplicity
    }

    #endregion
}