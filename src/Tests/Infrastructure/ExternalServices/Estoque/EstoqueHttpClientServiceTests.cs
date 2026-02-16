using System.Net;
using Application.OrdemServico.Dtos.External;
using FluentAssertions;
using Infrastructure.ExternalServices.Estoque;
using Shared.Enums;
using Shared.Exceptions;
using Tests.Application.OrdemServico.Helpers;
using Tests.Application.SharedHelpers;
using Tests.Infrastructure.ExternalServices.Http;
using Xunit;

namespace Tests.Infrastructure.ExternalServices.Estoque;

public class EstoqueHttpClientServiceTests
{
    #region ObterItemEstoquePorIdAsync

    [Fact(DisplayName = "Deve retornar DTO quando status 200")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterItemEstoquePorIdAsync_DeveRetornarDto_QuandoStatus200()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var itemId = Guid.NewGuid();
        var itemEsperado = new ItemEstoqueExternalDtoBuilder().ComId(itemId).Build();
        
        handler.ParaRota("GET", $"/api/estoque/itens/{itemId}")
               .Retornar(HttpStatusCode.OK, itemEsperado);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new EstoqueHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterItemEstoquePorIdAsync(itemId);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(itemEsperado.Id);
        resultado.Nome.Should().Be(itemEsperado.Nome);
        resultado.Preco.Should().Be(itemEsperado.Preco);
        resultado.Quantidade.Should().Be(itemEsperado.Quantidade);
        resultado.TipoItemEstoque.Should().Be(itemEsperado.TipoItemEstoque);
    }

    [Fact(DisplayName = "Deve retornar null quando status 404")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterItemEstoquePorIdAsync_DeveRetornarNull_QuandoStatus404()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var itemId = Guid.NewGuid();
        
        handler.ParaRota("GET", $"/api/estoque/itens/{itemId}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new EstoqueHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterItemEstoquePorIdAsync(itemId);
        
        // Assert
        resultado.Should().BeNull();
    }

    [Fact(DisplayName = "Deve lançar DomainException BadGateway quando status 500")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterItemEstoquePorIdAsync_DeveLancarDomainExceptionBadGateway_QuandoStatus500()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var itemId = Guid.NewGuid();
        
        handler.ParaRota("GET", $"/api/estoque/itens/{itemId}")
               .Retornar(HttpStatusCode.InternalServerError);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new EstoqueHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var act = async () => await service.ObterItemEstoquePorIdAsync(itemId);
        
        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.BadGateway);
        mockLogger.DeveTerLogadoError();
    }

    #endregion

    #region VerificarDisponibilidadeAsync

    [Fact(DisplayName = "Deve retornar false quando status 404")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task VerificarDisponibilidadeAsync_DeveRetornarFalse_QuandoStatus404()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 5;
        
        handler.ParaRota("GET", $"/api/estoque/itens/{itemId}/disponibilidade?quantidadeRequisitada={quantidadeNecessaria}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new EstoqueHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);
        
        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar true quando serviço retornar disponível true")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task VerificarDisponibilidadeAsync_DeveRetornarTrue_QuandoServicoRetornarDisponivelTrue()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 5;
        var disponibilidade = new DisponibilidadeExternalDto 
        { 
            Disponivel = true, 
            QuantidadeEmEstoque = 10, 
            QuantidadeSolicitada = quantidadeNecessaria 
        };
        
        handler.ParaRota("GET", $"/api/estoque/itens/{itemId}/disponibilidade?quantidadeRequisitada={quantidadeNecessaria}")
               .Retornar(HttpStatusCode.OK, disponibilidade);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new EstoqueHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);
        
        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando body for null ou disponível false")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task VerificarDisponibilidadeAsync_DeveRetornarFalse_QuandoBodyForNullOuDisponivelFalse()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 5;
        var disponibilidade = new DisponibilidadeExternalDto 
        { 
            Disponivel = false, 
            QuantidadeEmEstoque = 2, 
            QuantidadeSolicitada = quantidadeNecessaria 
        };
        
        handler.ParaRota("GET", $"/api/estoque/itens/{itemId}/disponibilidade?quantidadeRequisitada={quantidadeNecessaria}")
               .Retornar(HttpStatusCode.OK, disponibilidade);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new EstoqueHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);
        
        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve chamar endpoint com query string quantidade requisitada")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task VerificarDisponibilidadeAsync_DeveChamarEndpointComQueryStringQuantidadeRequisitada()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 5;
        var disponibilidade = new DisponibilidadeExternalDto 
        { 
            Disponivel = true, 
            QuantidadeEmEstoque = 10, 
            QuantidadeSolicitada = quantidadeNecessaria 
        };
        
        handler.ParaRota("GET", $"/api/estoque/itens/{itemId}/disponibilidade?quantidadeRequisitada={quantidadeNecessaria}")
               .Retornar(HttpStatusCode.OK, disponibilidade);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new EstoqueHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);
        
        // Assert
        resultado.Should().BeTrue();
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain($"quantidadeRequisitada={quantidadeNecessaria}");
    }

    #endregion
}
