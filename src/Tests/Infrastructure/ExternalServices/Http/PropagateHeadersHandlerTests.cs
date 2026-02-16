using Application.Contracts.Monitoramento;
using FluentAssertions;
using Infrastructure.ExternalServices.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using Xunit;

namespace Tests.Infrastructure.ExternalServices.Http;

public class PropagateHeadersHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;
    private readonly StubHttpMessageHandler _innerHandler;
    private readonly PropagateHeadersHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public PropagateHeadersHandlerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();
        _innerHandler = new StubHttpMessageHandler();
        
        _httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);
        
        _handler = new PropagateHeadersHandler(_httpContextAccessorMock.Object, _correlationIdAccessorMock.Object);
        _handler.InnerHandler = _innerHandler;
    }

    #region Authorization Header

    [Fact(DisplayName = "Deve propagar Authorization quando header presente")]
    [Trait("Infrastructure", "PropagateHeadersHandler")]
    public async Task SendAsync_DevePropagarAuthorization_QuandoHeaderPresente()
    {
        // Arrange
        var token = "meu-token-secreto";
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        _correlationIdAccessorMock.Setup(x => x.GetCorrelationId()).Returns(Guid.NewGuid().ToString());
        _innerHandler.ParaRota("GET", "/api/test").Retornar(HttpStatusCode.OK);

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        // Act
        await client.SendAsync(request);

        // Assert
        var requisicaoCapturada = _innerHandler.Requests.Should().ContainSingle().Subject;
        requisicaoCapturada.Headers.Authorization.Should().NotBeNull();
        requisicaoCapturada.Headers.Authorization!.Scheme.Should().Be("Bearer");
        requisicaoCapturada.Headers.Authorization.Parameter.Should().Be(token);
    }

    [Fact(DisplayName = "Não deve adicionar Authorization quando header ausente")]
    [Trait("Infrastructure", "PropagateHeadersHandler")]
    public async Task SendAsync_NaoDeveAdicionarAuthorization_QuandoHeaderAusente()
    {
        // Arrange
        _correlationIdAccessorMock.Setup(x => x.GetCorrelationId()).Returns(Guid.NewGuid().ToString());
        _innerHandler.ParaRota("GET", "/api/test").Retornar(HttpStatusCode.OK);

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        // Act
        await client.SendAsync(request);

        // Assert
        var requisicaoCapturada = _innerHandler.Requests.Should().ContainSingle().Subject;
        requisicaoCapturada.Headers.Authorization.Should().BeNull();
    }

    #endregion

    #region Correlation ID

    [Fact(DisplayName = "Deve adicionar Correlation ID quando chamado")]
    [Trait("Infrastructure", "PropagateHeadersHandler")]
    public async Task SendAsync_DeveAdicionarCorrelationId_QuandoChamado()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _correlationIdAccessorMock.Setup(x => x.GetCorrelationId()).Returns(correlationId);
        _innerHandler.ParaRota("GET", "/api/test").Retornar(HttpStatusCode.OK);

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        // Act
        await client.SendAsync(request);

        // Assert
        var requisicaoCapturada = _innerHandler.Requests.Should().ContainSingle().Subject;
        requisicaoCapturada.Headers.Should().Contain(h => h.Key == "X-Correlation-ID");
        var valorHeader = requisicaoCapturada.Headers.GetValues("X-Correlation-ID").Should().ContainSingle().Subject;
        valorHeader.Should().Be(correlationId);
    }

    [Fact(DisplayName = "Deve sobrescrever Correlation ID quando já existir")]
    [Trait("Infrastructure", "PropagateHeadersHandler")]
    public async Task SendAsync_DeveSobrescreverCorrelationId_QuandoJaExistir()
    {
        // Arrange
        var correlationIdAntigo = Guid.NewGuid().ToString();
        var correlationIdNovo = Guid.NewGuid().ToString();
        _correlationIdAccessorMock.Setup(x => x.GetCorrelationId()).Returns(correlationIdNovo);
        _innerHandler.ParaRota("GET", "/api/test").Retornar(HttpStatusCode.OK);

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
        request.Headers.Add("X-Correlation-ID", correlationIdAntigo);

        // Act
        await client.SendAsync(request);

        // Assert
        var requisicaoCapturada = _innerHandler.Requests.Should().ContainSingle().Subject;
        requisicaoCapturada.Headers.Should().Contain(h => h.Key == "X-Correlation-ID");
        var valorHeader = requisicaoCapturada.Headers.GetValues("X-Correlation-ID").Should().ContainSingle().Subject;
        valorHeader.Should().Be(correlationIdNovo);
    }

    #endregion
}
