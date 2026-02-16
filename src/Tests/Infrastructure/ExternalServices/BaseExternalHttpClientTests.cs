using FluentAssertions;
using Infrastructure.ExternalServices;
using Shared.Enums;
using Shared.Exceptions;
using System.Net;
using Tests.Application.SharedHelpers;
using Xunit;

namespace Tests.Infrastructure.ExternalServices;

public class BaseExternalHttpClientTests
{
    #region EnsureSuccessOrThrowAsync

    [Fact(DisplayName = "Não deve lançar exceção quando status sucesso")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task EnsureSuccessOrThrowAsync_NaoDeveLancarExcecao_QuandoStatusSucesso()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var mockLogger = MockLogger.Criar();

        // Act
        var act = async () => await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, "TesteOperacao", mockLogger.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory(DisplayName = "Deve lançar DomainException BadGateway quando status 500 ou maior")]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task EnsureSuccessOrThrowAsync_DeveLancarDomainExceptionBadGateway_QuandoStatus500OuMaior(int statusCode)
    {
        // Arrange
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent("Erro interno do servidor")
        };
        var mockLogger = MockLogger.Criar();

        // Act
        var act = async () => await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, "TesteOperacao", mockLogger.Object);

        // Assert
        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorType.Should().Be(ErrorType.BadGateway);
        exception.Which.Message.Should().Contain("TesteOperacao");
        mockLogger.DeveTerLogadoError();
    }

    [Theory(DisplayName = "Deve lançar DomainException com tipo mapeado quando status 4xx")]
    [InlineData(400, ErrorType.InvalidInput)]
    [InlineData(404, ErrorType.ResourceNotFound)]
    [InlineData(401, ErrorType.Unauthorized)]
    [InlineData(403, ErrorType.NotAllowed)]
    [InlineData(409, ErrorType.Conflict)]
    [InlineData(422, ErrorType.DomainRuleBroken)]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task EnsureSuccessOrThrowAsync_DeveLancarDomainExceptionComTipoMapeado_QuandoStatus4xx(int statusCode, ErrorType errorTypeEsperado)
    {
        // Arrange
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent("Erro do cliente")
        };
        var mockLogger = MockLogger.Criar();

        // Act
        var act = async () => await BaseExternalHttpClient.EnsureSuccessOrThrowAsync(response, "TesteOperacao", mockLogger.Object);

        // Assert
        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorType.Should().Be(errorTypeEsperado);
        exception.Which.Message.Should().Contain("TesteOperacao");
        mockLogger.DeveTerLogadoInformation();
    }

    #endregion

    #region ExecuteHttpOperationAsync

    [Fact(DisplayName = "Deve retornar resposta quando operação for bem-sucedida")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ExecuteHttpOperationAsync_DeveRetornarResposta_QuandoOperacaoForBemSucedida()
    {
        // Arrange
        var respostaEsperada = new HttpResponseMessage(HttpStatusCode.OK);
        var mockLogger = MockLogger.Criar();

        // Act
        var resultado = await BaseExternalHttpClient.ExecuteHttpOperationAsync(() => Task.FromResult(respostaEsperada), "TesteOperacao", mockLogger.Object);

        // Assert
        resultado.Should().NotBeNull();
        resultado.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Deve lançar DomainException BadGateway quando HttpRequestException")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ExecuteHttpOperationAsync_DeveLancarDomainExceptionBadGateway_QuandoHttpRequestException()
    {
        // Arrange
        var mockLogger = MockLogger.Criar();
        Task<HttpResponseMessage> operacaoComFalha() => throw new HttpRequestException("Falha de conectividade");

        // Act
        var act = async () => await BaseExternalHttpClient.ExecuteHttpOperationAsync(operacaoComFalha, "TesteOperacao", mockLogger.Object);

        // Assert
        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorType.Should().Be(ErrorType.BadGateway);
        exception.Which.Message.Should().Contain("TesteOperacao");
        mockLogger.DeveTerLogadoErrorComException();
    }

    [Fact(DisplayName = "Deve lançar DomainException BadGateway quando timeout")]
    [Trait("Infrastructure", "ExternalServices")]
    public async Task ExecuteHttpOperationAsync_DeveLancarDomainExceptionBadGateway_QuandoTimeout()
    {
        // Arrange
        var mockLogger = MockLogger.Criar();
        var timeoutException = new TimeoutException("A operação expirou.");
        Task<HttpResponseMessage> operacaoComTimeout() => throw new TaskCanceledException("A requisição foi cancelada devido a timeout.", timeoutException);

        // Act
        var act = async () => await BaseExternalHttpClient.ExecuteHttpOperationAsync(operacaoComTimeout, "TesteOperacao", mockLogger.Object);

        // Assert
        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorType.Should().Be(ErrorType.BadGateway);
        exception.Which.Message.Should().Contain("TesteOperacao");
        mockLogger.DeveTerLogadoErrorComException();
    }

    #endregion
}
