using Application.OrdemServico.Dtos.External;
using Infrastructure.ExternalServices.Cadastro;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Tests.Infrastructure.ExternalServices.Cadastro;

/// <summary>
/// Testes unitários para CadastroHttpClientService.
/// Verifica a comunicação HTTP com o microsserviço de Cadastros e propagação de headers.
/// </summary>
public class CadastroHttpClientServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public CadastroHttpClientServiceTests()
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

    #region ObterServicoPorIdAsync Tests

    [Fact]
    public async Task ObterServicoPorIdAsync_WhenServiceExists_ReturnsDTO()
    {
        // Arrange
        var servicoId = Guid.NewGuid();
        var expectedDto = new ServicoExternalDto
        {
            Id = servicoId,
            Nome = "Troca de óleo",
            Preco = 150.00m
        };

        SetupHttpContext("test-token-123", "correlation-abc");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/cadastros/servicos/{servicoId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedDto)
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.ObterServicoPorIdAsync(servicoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Nome, result.Nome);
        Assert.Equal(expectedDto.Preco, result.Preco);
    }

    [Fact]
    public async Task ObterServicoPorIdAsync_WhenServiceNotFound_ReturnsNull()
    {
        // Arrange
        var servicoId = Guid.NewGuid();
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

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.ObterServicoPorIdAsync(servicoId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObterServicoPorIdAsync_PropagatesAuthToken()
    {
        // Arrange
        var servicoId = Guid.NewGuid();
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
                Content = JsonContent.Create(new ServicoExternalDto 
                { 
                    Id = servicoId, 
                    Nome = "Test", 
                    Preco = 100 
                })
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        await service.ObterServicoPorIdAsync(servicoId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(expectedToken, capturedRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task ObterServicoPorIdAsync_PropagatesCorrelationId()
    {
        // Arrange
        var servicoId = Guid.NewGuid();
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
                Content = JsonContent.Create(new ServicoExternalDto 
                { 
                    Id = servicoId, 
                    Nome = "Test", 
                    Preco = 100 
                })
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        await service.ObterServicoPorIdAsync(servicoId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("X-Correlation-ID"));
        var correlationId = capturedRequest.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(expectedCorrelationId, correlationId);
    }

    #endregion

    #region VerificarExistenciaVeiculo Tests

    [Fact]
    public async Task VerificarExistenciaVeiculo_WhenVeiculoExists_ReturnsTrue()
    {
        // Arrange
        var veiculoId = Guid.NewGuid();
        SetupHttpContext("test-token");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/cadastros/veiculos/{veiculoId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.VerificarExistenciaVeiculo(veiculoId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerificarExistenciaVeiculo_WhenVeiculoNotFound_ReturnsFalse()
    {
        // Arrange
        var veiculoId = Guid.NewGuid();
        SetupHttpContext("test-token");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.VerificarExistenciaVeiculo(veiculoId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CriarClienteAsync Tests

    [Fact]
    public async Task CriarClienteAsync_WhenSuccessful_ReturnsCreatedDTO()
    {
        // Arrange
        var criarDto = new CriarClienteExternalDto
        {
            Nome = "João Silva",
            DocumentoIdentificador = "12345678900"
        };

        var expectedDto = new ClienteExternalDto
        {
            Id = Guid.NewGuid(),
            Nome = criarDto.Nome,
            DocumentoIdentificador = criarDto.DocumentoIdentificador
        };

        SetupHttpContext("test-token-123", "correlation-def");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/api/cadastros/clientes")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(expectedDto)
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.CriarClienteAsync(criarDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Nome, result.Nome);
        Assert.Equal(expectedDto.DocumentoIdentificador, result.DocumentoIdentificador);
    }

    [Fact]
    public async Task CriarClienteAsync_PropagatesAuthTokenAndCorrelationId()
    {
        // Arrange
        var criarDto = new CriarClienteExternalDto
        {
            Nome = "João Silva",
            DocumentoIdentificador = "12345678900"
        };

        var expectedToken = "test-create-token";
        var expectedCorrelationId = "correlation-create-123";

        SetupHttpContext(expectedToken, expectedCorrelationId);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(new ClienteExternalDto
                {
                    Id = Guid.NewGuid(),
                    Nome = criarDto.Nome,
                    DocumentoIdentificador = criarDto.DocumentoIdentificador
                })
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        await service.CriarClienteAsync(criarDto);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(expectedToken, capturedRequest.Headers.Authorization.Parameter);
        
        Assert.True(capturedRequest.Headers.Contains("X-Correlation-ID"));
        var correlationId = capturedRequest.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(expectedCorrelationId, correlationId);
    }

    #endregion

    #region ObterClientePorVeiculoIdAsync Tests

    [Fact]
    public async Task ObterClientePorVeiculoIdAsync_WhenVeiculoAndClienteExist_ReturnsClienteDTO()
    {
        // Arrange
        var veiculoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        var veiculoDto = new VeiculoExternalDto
        {
            Id = veiculoId,
            ClienteId = clienteId,
            Placa = "ABC1234",
            Modelo = "Civic",
            Marca = "Honda",
            Cor = "Preto",
            Ano = 2020,
            TipoVeiculo = 1
        };

        var clienteDto = new ClienteExternalDto
        {
            Id = clienteId,
            Nome = "Maria Santos",
            DocumentoIdentificador = "98765432100"
        };

        SetupHttpContext("test-token");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/cadastros/veiculos/{veiculoId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(veiculoDto)
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/cadastros/clientes/{clienteId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(clienteDto)
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.ObterClientePorVeiculoIdAsync(veiculoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clienteDto.Id, result.Id);
        Assert.Equal(clienteDto.Nome, result.Nome);
        Assert.Equal(clienteDto.DocumentoIdentificador, result.DocumentoIdentificador);
    }

    [Fact]
    public async Task ObterClientePorVeiculoIdAsync_WhenVeiculoNotFound_ReturnsNull()
    {
        // Arrange
        var veiculoId = Guid.NewGuid();
        SetupHttpContext("test-token");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.ObterClientePorVeiculoIdAsync(veiculoId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CriarVeiculoAsync Tests

    [Fact]
    public async Task CriarVeiculoAsync_WhenSuccessful_ReturnsCreatedDTO()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var criarDto = new CriarVeiculoExternalDto
        {
            ClienteId = clienteId,
            Placa = "XYZ5678",
            Modelo = "Corolla",
            Marca = "Toyota",
            Cor = "Branco",
            Ano = 2022,
            TipoVeiculo = 1
        };

        var expectedDto = new VeiculoExternalDto
        {
            Id = Guid.NewGuid(),
            ClienteId = criarDto.ClienteId,
            Placa = criarDto.Placa,
            Modelo = criarDto.Modelo,
            Marca = criarDto.Marca,
            Cor = criarDto.Cor,
            Ano = criarDto.Ano,
            TipoVeiculo = criarDto.TipoVeiculo
        };

        SetupHttpContext("test-token-456");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/api/cadastros/veiculos")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(expectedDto)
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.CriarVeiculoAsync(criarDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Placa, result.Placa);
        Assert.Equal(expectedDto.Modelo, result.Modelo);
    }

    #endregion

    #region ObterVeiculoPorPlacaAsync Tests

    [Fact]
    public async Task ObterVeiculoPorPlacaAsync_WhenVeiculoExists_ReturnsDTO()
    {
        // Arrange
        var placa = "ABC1234";
        var expectedDto = new VeiculoExternalDto
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Placa = placa,
            Modelo = "Civic",
            Marca = "Honda",
            Cor = "Preto",
            Ano = 2020,
            TipoVeiculo = 1
        };

        SetupHttpContext("test-token");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/cadastros/veiculos/placa/{Uri.EscapeDataString(placa)}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedDto)
            });

        var service = new CadastroHttpClientService(_httpClient, _httpContextAccessorMock.Object);

        // Act
        var result = await service.ObterVeiculoPorPlacaAsync(placa);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Placa, result.Placa);
    }

    #endregion
}
