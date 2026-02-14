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
/// Testes de integração para endpoints webhook e adição/remoção de itens/serviços
/// </summary>
public class OrdemServicoControllerWebhookTests : IClassFixture<Mongo2GoFixture>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _clientAdmin;
    private readonly HttpClient _clientUnauth;

    public OrdemServicoControllerWebhookTests(Mongo2GoFixture fixture)
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

    #region POST /api/ordens-servico/{id}/servicos

    [Fact(DisplayName = "Deve adicionar serviços com sucesso")]
    [Trait("Method", "POST")]
    public async Task AdicionarServicos_OrdemEmDiagnostico_DeveRetornar200()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.EmDiagnostico();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var servicoId = Guid.NewGuid();
        var request = OrdemServicoRequestBuilder.AdicionarServicos(servicoId);
        
        _factory.Mocks.ServicoService
            .Setup(x => x.ObterServicoPorIdAsync(servicoId))
            .ReturnsAsync(new ServicoExternalDto
            {
                Id = servicoId,
                Nome = "Troca de óleo",
                Preco = 50.00m
            });

        // Act
        var response = await _clientAdmin.PostAsJsonAsync($"/api/ordens-servico/{ordemServico.Id}/servicos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se serviço foi adicionado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.ServicosIncluidos.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task AdicionarServicos_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = OrdemServicoRequestBuilder.AdicionarServicos(Guid.NewGuid());

        // Act
        var response = await _clientAdmin.PostAsJsonAsync($"/api/ordens-servico/{idInexistente}/servicos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/ordens-servico/{id}/itens

    [Fact(DisplayName = "Deve adicionar item com sucesso")]
    [Trait("Method", "POST")]
    public async Task AdicionarItem_OrdemEmDiagnostico_DeveRetornar200()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.EmDiagnostico();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.AdicionarItem();
        
        _factory.Mocks.EstoqueService
            .Setup(x => x.ObterItemEstoquePorIdAsync(request.ItemEstoqueOriginalId))
            .ReturnsAsync(new ItemEstoqueExternalDto
            {
                Id = request.ItemEstoqueOriginalId,
                Nome = "Filtro de óleo",
                Preco = 25.00m,
                Quantidade = 1,
                TipoItemIncluido = TipoItemIncluidoEnum.Peca
            });

        // Act
        var response = await _clientAdmin.PostAsJsonAsync($"/api/ordens-servico/{ordemServico.Id}/itens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se item foi adicionado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.ItensIncluidos.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "POST")]
    public async Task AdicionarItem_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = OrdemServicoRequestBuilder.AdicionarItem();

        // Act
        var response = await _clientAdmin.PostAsJsonAsync($"/api/ordens-servico/{idInexistente}/itens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/ordens-servico/{id}/servicos/{servicoIncluidoId}

    [Fact(DisplayName = "Deve remover serviço com sucesso")]
    [Trait("Method", "DELETE")]
    public async Task RemoverServico_ServicoExistente_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.ComServicos(1);
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var servicoIncluido = ordemServico.ServicosIncluidos.First();

        // Act
        var response = await _clientAdmin.DeleteAsync($"/api/ordens-servico/{ordemServico.Id}/servicos/{servicoIncluido.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se serviço foi removido
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.ServicosIncluidos.Should().BeEmpty();
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "DELETE")]
    public async Task RemoverServico_IdInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var servicoIdInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.DeleteAsync($"/api/ordens-servico/{idInexistente}/servicos/{servicoIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/ordens-servico/{id}/itens/{itemIncluidoId}

    [Fact(DisplayName = "Deve remover item com sucesso")]
    [Trait("Method", "DELETE")]
    public async Task RemoverItem_ItemExistente_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.ComItens(1);
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var itemIncluido = ordemServico.ItensIncluidos.First();

        // Act
        var response = await _clientAdmin.DeleteAsync($"/api/ordens-servico/{ordemServico.Id}/itens/{itemIncluido.Id}");

        // Assert  
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se item foi removido
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.ItensIncluidos.Should().BeEmpty();
    }

    [Fact(DisplayName = "Deve retornar 404 para ID inexistente")]
    [Trait("Method", "DELETE")]
    public async Task RemoverItem_IdInexistente_DeveRetornar404()
    {   
        // Arrange
        var idInexistente = Guid.NewGuid();
        var itemIdInexistente = Guid.NewGuid();

        // Act
        var response = await _clientAdmin.DeleteAsync($"/api/ordens-servico/{idInexistente}/itens/{itemIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Webhook Tests

    [Fact(DisplayName = "Deve processar webhook de aprovar orçamento com assinatura HMAC válida")]
    [Trait("Method", "POST")]
    public async Task WebhookAprovarOrcamento_AssinaturaValida_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.AguardandoAprovacao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.WebhookComId(ordemServico.Id);

        // Act  
        var response = await _clientUnauth.PostAsJsonWithHmacAsync("/api/ordens-servico/orcamento/aprovar/webhook", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
    }

    [Fact(DisplayName = "Deve retornar 401 para webhook sem assinatura HMAC")]
    [Trait("Method", "POST")]
    public async Task WebhookAprovarOrcamento_SemAssinatura_DeveRetornar401()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.AguardandoAprovacao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.WebhookComId(ordemServico.Id);

        // Act - send without HMAC signature
        var response = await _clientUnauth.PostAsJsonAsync("/api/ordens-servico/orcamento/aprovar/webhook", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Verificar que status NÃO foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.AguardandoAprovacao);
    }

    [Fact(DisplayName = "Deve processar webhook de desaprovar orçamento com assinatura HMAC válida")]
    [Trait("Method", "POST")]
    public async Task WebhookDesaprovarOrcamento_AssinaturaValida_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.AguardandoAprovacao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.WebhookComId(ordemServico.Id);

        // Act
        var response = await _clientUnauth.PostAsJsonWithHmacAsync("/api/ordens-servico/orcamento/desaprovar/webhook", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
    }

    [Fact(DisplayName = "Deve retornar 401 para webhook desaprovar sem assinatura HMAC")]
    [Trait("Method", "POST")]
    public async Task WebhookDesaprovarOrcamento_SemAssinatura_DeveRetornar401()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.AguardandoAprovacao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.WebhookComId(ordemServico.Id);

        // Act - send without HMAC signature
        var response = await _clientUnauth.PostAsJsonAsync("/api/ordens-servico/orcamento/desaprovar/webhook", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Verificar que status NÃO foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.AguardandoAprovacao);
    }

    [Fact(DisplayName = "Deve processar webhook de alterar status com assinatura HMAC válida")]
    [Trait("Method", "POST")]
    public async Task WebhookAlterarStatus_AssinaturaValida_DeveRetornar204()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.EmExecucao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.WebhookAlterarStatus(ordemServico.Id, StatusOrdemServicoEnum.Finalizada);

        // Act
        var response = await _clientUnauth.PostAsJsonWithHmacAsync("/api/ordens-servico/status/webhook", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar se status foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Finalizada);
    }

    [Fact(DisplayName = "Deve retornar 401 para webhook alterar status sem assinatura HMAC")]
    [Trait("Method", "POST")]
    public async Task WebhookAlterarStatus_SemAssinatura_DeveRetornar401()
    {
        // Arrange
        var ordemServico = OrdemServicoAggregateBuilder.EmExecucao();
        await MongoOrdemServicoTestHelper.InsertSeedAsync(_factory.Services, ordemServico);

        var request = OrdemServicoRequestBuilder.WebhookAlterarStatus(ordemServico.Id, StatusOrdemServicoEnum.Finalizada);

        // Act - send without HMAC signature
        var response = await _clientUnauth.PostAsJsonAsync("/api/ordens-servico/status/webhook", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Verificar que status NÃO foi alterado
        var ordemServicoDb = await MongoOrdemServicoTestHelper.FindByIdAsync(_factory.Services, ordemServico.Id);
        ordemServicoDb.Should().NotBeNull();
        ordemServicoDb!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
    }

    #endregion
}