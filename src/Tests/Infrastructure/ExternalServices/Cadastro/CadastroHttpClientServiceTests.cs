using Application.OrdemServico.Dtos.External;
using FluentAssertions;
using Infrastructure.ExternalServices.Cadastro;
using Shared.Exceptions;
using System.Net;
using Tests.Application.OrdemServico.Helpers;
using Tests.Application.SharedHelpers;
using Tests.Application.SharedHelpers.AggregateBuilders;
using Tests.Infrastructure.ExternalServices.Http;
using Xunit;

namespace Tests.Infrastructure.ExternalServices.Cadastro;

public class CadastroHttpClientServiceTests
{
    #region IServicoExternalService

    [Fact(DisplayName = "Deve retornar DTO quando status 200")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterServicoPorIdAsync_DeveRetornarDto_QuandoStatus200()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var servicoId = Guid.NewGuid();
        var servicoEsperado = new ServicoExternalDtoBuilder().ComId(servicoId).Build();
        
        handler.ParaRota("GET", $"/api/servicos/{servicoId}")
               .Retornar(HttpStatusCode.OK, servicoEsperado);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterServicoPorIdAsync(servicoId);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(servicoEsperado.Id);
        resultado.Nome.Should().Be(servicoEsperado.Nome);
        resultado.Preco.Should().Be(servicoEsperado.Preco);
    }

    [Fact(DisplayName = "Deve retornar null quando status 404")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterServicoPorIdAsync_DeveRetornarNull_QuandoStatus404()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var servicoId = Guid.NewGuid();
        
        handler.ParaRota("GET", $"/api/servicos/{servicoId}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterServicoPorIdAsync(servicoId);
        
        // Assert
        resultado.Should().BeNull();
    }

    [Fact(DisplayName = "Deve lançar DomainException BadGateway quando status 500")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterServicoPorIdAsync_DeveLancarDomainExceptionBadGateway_QuandoStatus500()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var servicoId = Guid.NewGuid();
        
        handler.ParaRota("GET", $"/api/servicos/{servicoId}")
               .Retornar(HttpStatusCode.InternalServerError);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var act = () => service.ObterServicoPorIdAsync(servicoId);
        
        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == Shared.Enums.ErrorType.BadGateway);
        mockLogger.DeveTerLogadoError();
    }

    #endregion

    #region IVeiculoExternalService

    [Fact(DisplayName = "Deve retornar true quando status 200")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task VerificarExistenciaVeiculo_DeveRetornarTrue_QuandoStatus200()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var veiculoId = Guid.NewGuid();
        var veiculo = new VeiculoExternalDtoBuilder().Build();
        
        handler.ParaRota("GET", $"/api/veiculos/{veiculoId}")
               .Retornar(HttpStatusCode.OK, veiculo);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.VerificarExistenciaVeiculo(veiculoId);
        
        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando status 404")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task VerificarExistenciaVeiculo_DeveRetornarFalse_QuandoStatus404()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var veiculoId = Guid.NewGuid();
        
        handler.ParaRota("GET", $"/api/veiculos/{veiculoId}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.VerificarExistenciaVeiculo(veiculoId);
        
        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar DTO quando status 200")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterVeiculoPorIdAsync_DeveRetornarDto_QuandoStatus200()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var veiculoId = Guid.NewGuid();
        var veiculoEsperado = new VeiculoExternalDtoBuilder().Build();
        
        handler.ParaRota("GET", $"/api/veiculos/{veiculoId}")
               .Retornar(HttpStatusCode.OK, veiculoEsperado);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterVeiculoPorIdAsync(veiculoId);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado!.Placa.Should().Be(veiculoEsperado.Placa);
        resultado.Modelo.Should().Be(veiculoEsperado.Modelo);
        resultado.Marca.Should().Be(veiculoEsperado.Marca);
    }

    [Fact(DisplayName = "Deve retornar null quando status 404")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterVeiculoPorIdAsync_DeveRetornarNull_QuandoStatus404()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var veiculoId = Guid.NewGuid();
        
        handler.ParaRota("GET", $"/api/veiculos/{veiculoId}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterVeiculoPorIdAsync(veiculoId);
        
        // Assert
        resultado.Should().BeNull();
    }

    [Fact(DisplayName = "Deve escapar placa na URL quando possuir caracteres especiais")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterVeiculoPorPlacaAsync_DeveEscaparPlacaNaUrl_QuandoPossuirCaracteresEspeciais()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var placaComEspaco = "ABC 1234";
        var placaEscapada = Uri.EscapeDataString(placaComEspaco);
        var veiculoEsperado = new VeiculoExternalDtoBuilder().ComPlaca(placaComEspaco).Build();
        
        handler.ParaRota("GET", $"/api/veiculos/placa/{placaEscapada}")
               .Retornar(HttpStatusCode.OK, veiculoEsperado);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterVeiculoPorPlacaAsync(placaComEspaco);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado!.Placa.Should().Be(placaComEspaco);
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain(placaEscapada);
    }

    [Theory(DisplayName = "Deve retornar DTO quando status 200 ou 201")]
    [InlineData(200)]
    [InlineData(201)]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task CriarVeiculoAsync_DeveRetornarDto_QuandoStatus200Ou201(int statusCode)
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var criarDto = new CriarVeiculoExternalDto
        {
            ClienteId = Guid.NewGuid(),
            Placa = "ABC-1234",
            Modelo = "Civic",
            Marca = "Honda",
            Cor = "Preto",
            Ano = 2023,
            TipoVeiculo = 1
        };
        var veiculoEsperado = new VeiculoExternalDtoBuilder()
            .ComClienteId(criarDto.ClienteId)
            .ComPlaca(criarDto.Placa)
            .ComModelo(criarDto.Modelo)
            .ComMarca(criarDto.Marca)
            .Build();
        
        handler.ParaRota("POST", "/api/veiculos")
               .Retornar((HttpStatusCode)statusCode, veiculoEsperado);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.CriarVeiculoAsync(criarDto);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado.Placa.Should().Be(criarDto.Placa);
        resultado.Modelo.Should().Be(criarDto.Modelo);
    }

    [Fact(DisplayName = "Deve lançar JsonException quando resposta vazia")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task CriarVeiculoAsync_DeveLancarJsonException_QuandoRespostaVazia()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var criarDto = new CriarVeiculoExternalDto
        {
            ClienteId = Guid.NewGuid(),
            Placa = "ABC-1234",
            Modelo = "Civic",
            Marca = "Honda",
            Cor = "Preto",
            Ano = 2023,
            TipoVeiculo = 1
        };
        
        handler.ParaRota("POST", "/api/veiculos")
               .Retornar(HttpStatusCode.OK);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var act = () => service.CriarVeiculoAsync(criarDto);
        
        // Assert
        await act.Should().ThrowAsync<System.Text.Json.JsonException>();
    }

    #endregion

    #region IClienteExternalService

    [Fact(DisplayName = "Deve retornar null quando veículo não encontrado")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterClientePorVeiculoIdAsync_DeveRetornarNull_QuandoVeiculoNaoEncontrado()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var veiculoId = Guid.NewGuid();
        
        handler.ParaRota("GET", $"/api/veiculos/{veiculoId}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterClientePorVeiculoIdAsync(veiculoId);
        
        // Assert
        resultado.Should().BeNull();
        handler.Requests.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Deve retornar DTO quando veículo e cliente existem")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterClientePorVeiculoIdAsync_DeveRetornarDto_QuandoVeiculoEClienteExistem()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var veiculoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var veiculoEsperado = new VeiculoExternalDtoBuilder().ComClienteId(clienteId).Build();
        var clienteEsperado = new ClienteExternalDtoBuilder().ComId(clienteId).Build();
        
        handler.ParaRota("GET", $"/api/veiculos/{veiculoId}")
               .Retornar(HttpStatusCode.OK, veiculoEsperado);
        
        handler.ParaRota("GET", $"/api/clientes/{clienteId}")
               .Retornar(HttpStatusCode.OK, clienteEsperado);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterClientePorVeiculoIdAsync(veiculoId);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(clienteEsperado.Id);
        resultado.Nome.Should().Be(clienteEsperado.Nome);
        resultado.DocumentoIdentificador.Should().Be(clienteEsperado.DocumentoIdentificador);
        handler.Requests.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Deve retornar null quando cliente não encontrado")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterClientePorVeiculoIdAsync_DeveRetornarNull_QuandoClienteNaoEncontrado()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var veiculoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var veiculoEsperado = new VeiculoExternalDtoBuilder().ComClienteId(clienteId).Build();
        
        handler.ParaRota("GET", $"/api/veiculos/{veiculoId}")
               .Retornar(HttpStatusCode.OK, veiculoEsperado);
        
        handler.ParaRota("GET", $"/api/clientes/{clienteId}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterClientePorVeiculoIdAsync(veiculoId);
        
        // Assert
        resultado.Should().BeNull();
        handler.Requests.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Deve retornar null quando status 404")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ObterPorDocumentoAsync_DeveRetornarNull_QuandoStatus404()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var documento = "12345678901";
        var documentoEscapado = Uri.EscapeDataString(documento);
        
        handler.ParaRota("GET", $"/api/clientes/documento/{documentoEscapado}")
               .Retornar(HttpStatusCode.NotFound);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.ObterPorDocumentoAsync(documento);
        
        // Assert
        resultado.Should().BeNull();
    }

    [Theory(DisplayName = "Deve retornar DTO quando status 200 ou 201")]
    [InlineData(200)]
    [InlineData(201)]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task CriarClienteAsync_DeveRetornarDto_QuandoStatus200Ou201(int statusCode)
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var criarDto = new CriarClienteExternalDto
        {
            Nome = "João Silva",
            DocumentoIdentificador = "12345678901"
        };
        var clienteEsperado = new ClienteExternalDtoBuilder()
            .ComNome(criarDto.Nome)
            .ComDocumentoIdentificador(criarDto.DocumentoIdentificador)
            .Build();
        
        handler.ParaRota("POST", "/api/clientes")
               .Retornar((HttpStatusCode)statusCode, clienteEsperado);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var resultado = await service.CriarClienteAsync(criarDto);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(criarDto.Nome);
        resultado.DocumentoIdentificador.Should().Be(criarDto.DocumentoIdentificador);
    }

    [Fact(DisplayName = "Deve lançar JsonException quando resposta vazia")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task CriarClienteAsync_DeveLancarJsonException_QuandoRespostaVazia()
    {
        // Arrange
        var handler = new StubHttpMessageHandler();
        var criarDto = new CriarClienteExternalDto
        {
            Nome = "João Silva",
            DocumentoIdentificador = "12345678901"
        };
        
        handler.ParaRota("POST", "/api/clientes")
               .Retornar(HttpStatusCode.OK);
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var mockLogger = MockLogger.Criar();
        var service = new CadastroHttpClientService(httpClient, mockLogger.Object);
        
        // Act
        var act = () => service.CriarClienteAsync(criarDto);
        
        // Assert
        await act.Should().ThrowAsync<System.Text.Json.JsonException>();
    }

    #endregion
}
