using Application.OrdemServico.Dtos.External;
using Domain.OrdemServico.Enums;
using Infrastructure.ExternalServices.Estoque;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Tests.Infrastructure.ExternalServices.Estoque;

/// <summary>
/// Testes unitários para EstoqueHttpClientService.
/// Verifica a comunicação HTTP com o microsserviço de Estoque e propagação de headers.
/// </summary>
public class EstoqueHttpClientServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public EstoqueHttpClientServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    private void SetupHttpContext(string? authToken = null, string? correlationId = null)
    {
        var context = new DefaultHttpContext();
        
        if (!string.IsNullOrEmpty(authToken))
        {
            context.Request.Headers["Authorization"] = $"Bearer {authToken}";
        }
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            context.Request.Headers["X-Correlation-ID"] = correlationId;
        }
        
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
    }

    #region ObterItemEstoquePorIdAsync Tests

    [Fact]
    public async Task ObterItemEstoquePorIdAsync_WhenItemExists_ReturnsDTO()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedDto = new ItemEstoqueExternalDto
        {
            Id = itemId,
            Nome = "Filtro de óleo",
            Preco = 45.90m,
            Quantidade = 20,
            TipoItemIncluido = TipoItemIncluidoEnum.Peca
        };

        SetupHttpContext("test-token-123", "correlation-abc");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/estoque/itens/{itemId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedDto)
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.ObterItemEstoquePorIdAsync(itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Nome, result.Nome);
        Assert.Equal(expectedDto.Preco, result.Preco);
        Assert.Equal(expectedDto.Quantidade, result.Quantidade);
        Assert.Equal(expectedDto.TipoItemIncluido, result.TipoItemIncluido);
    }

    [Fact]
    public async Task ObterItemEstoquePorIdAsync_WhenItemNotFound_ReturnsNull()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        SetupHttpContext("test-token-123");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.ObterItemEstoquePorIdAsync(itemId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObterItemEstoquePorIdAsync_PropagatesAuthToken()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedToken = "test-bearer-token";
        SetupHttpContext(expectedToken);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new ItemEstoqueExternalDto 
                { 
                    Id = itemId, 
                    Nome = "Test", 
                    Preco = 100,
                    Quantidade = 10,
                    TipoItemIncluido = TipoItemIncluidoEnum.Peca
                })
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        await service.ObterItemEstoquePorIdAsync(itemId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(expectedToken, capturedRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task ObterItemEstoquePorIdAsync_PropagatesCorrelationId()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedCorrelationId = "correlation-xyz-789";
        SetupHttpContext(correlationId: expectedCorrelationId);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new ItemEstoqueExternalDto 
                { 
                    Id = itemId, 
                    Nome = "Test", 
                    Preco = 100,
                    Quantidade = 10,
                    TipoItemIncluido = TipoItemIncluidoEnum.Peca
                })
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        await service.ObterItemEstoquePorIdAsync(itemId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("X-Correlation-ID"));
        var correlationId = capturedRequest.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(expectedCorrelationId, correlationId);
    }

    #endregion

    #region VerificarDisponibilidadeAsync Tests

    [Fact]
    public async Task VerificarDisponibilidadeAsync_WhenAvailable_ReturnsTrue()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 5;

        var disponibilidadeDto = new DisponibilidadeExternalDto
        {
            Disponivel = true,
            QuantidadeEmEstoque = 20,
            QuantidadeSolicitada = quantidadeNecessaria
        };

        SetupHttpContext("test-token-456", "correlation-def");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/estoque/itens/{itemId}/disponibilidade") &&
                    req.RequestUri.ToString().Contains($"quantidadeRequisitada={quantidadeNecessaria}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(disponibilidadeDto)
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_WhenNotAvailable_ReturnsFalse()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 100;

        var disponibilidadeDto = new DisponibilidadeExternalDto
        {
            Disponivel = false,
            QuantidadeEmEstoque = 5,
            QuantidadeSolicitada = quantidadeNecessaria
        };

        SetupHttpContext("test-token-789");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/estoque/itens/{itemId}/disponibilidade") &&
                    req.RequestUri.ToString().Contains($"quantidadeRequisitada={quantidadeNecessaria}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(disponibilidadeDto)
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_WhenItemNotFound_ReturnsFalse()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 10;

        SetupHttpContext("test-token-notfound");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_WhenResponseHasNullDisponivel_ReturnsFalse()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 10;

        SetupHttpContext("test-token-null");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create<DisponibilidadeExternalDto?>(null)
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_PropagatesAuthToken()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 10;
        var expectedToken = "test-disponibilidade-token";
        SetupHttpContext(expectedToken);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new DisponibilidadeExternalDto
                {
                    Disponivel = true,
                    QuantidadeEmEstoque = 50,
                    QuantidadeSolicitada = quantidadeNecessaria
                })
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(expectedToken, capturedRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_PropagatesCorrelationId()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var quantidadeNecessaria = 10;
        var expectedCorrelationId = "correlation-disponibilidade-123";
        SetupHttpContext(correlationId: expectedCorrelationId);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new DisponibilidadeExternalDto
                {
                    Disponivel = true,
                    QuantidadeEmEstoque = 50,
                    QuantidadeSolicitada = quantidadeNecessaria
                })
            });

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        await service.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("X-Correlation-ID"));
        var correlationId = capturedRequest.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(expectedCorrelationId, correlationId);
    }

    #endregion

    #region AtualizarQuantidadeEstoqueAsync Tests

    [Fact]
    public async Task AtualizarQuantidadeEstoqueAsync_ThrowsNotImplementedException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var novaQuantidade = 50;
        SetupHttpContext("test-token");

        var service = new EstoqueHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(
            () => service.AtualizarQuantidadeEstoqueAsync(itemId, novaQuantidade));

        Assert.Contains("não é implementado via REST", exception.Message);
        Assert.Contains("mensageria assíncrona", exception.Message);
        Assert.Contains("SQS", exception.Message);
    }

    #endregion
}
