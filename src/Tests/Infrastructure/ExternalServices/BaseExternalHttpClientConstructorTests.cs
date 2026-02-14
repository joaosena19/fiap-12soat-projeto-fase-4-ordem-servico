using Application.Contracts.Monitoramento;
using FluentAssertions;
using Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Tests.Infrastructure.ExternalServices;

public class BaseExternalHttpClientConstructorTests
{
    #region Teste Implementacao Concreta

    private class BaseExternalHttpClientTestavel : BaseExternalHttpClient
    {
        public BaseExternalHttpClientTestavel(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ICorrelationIdAccessor correlationIdAccessor) : base(httpClient, httpContextAccessor, correlationIdAccessor)
        {
        }
    }

    #endregion

    #region Validacoes Construtor

    [Fact(DisplayName = "Não deve criar instância quando HttpClient for nulo")]
    [Trait("Infrastructure", "BaseExternalHttpClient")]
    public void Construtor_DeveLancarArgumentNullException_QuandoHttpClientNulo()
    {
        // Arrange
        HttpClient? httpClientNulo = null;
        var httpContextAccessor = new Mock<IHttpContextAccessor>().Object;
        var correlationIdAccessor = new Mock<ICorrelationIdAccessor>().Object;

        // Act & Assert
        FluentActions.Invoking(() => new BaseExternalHttpClientTestavel(httpClientNulo!, httpContextAccessor, correlationIdAccessor))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact(DisplayName = "Não deve criar instância quando HttpContextAccessor for nulo")]
    [Trait("Infrastructure", "BaseExternalHttpClient")]
    public void Construtor_DeveLancarArgumentNullException_QuandoHttpContextAccessorNulo()
    {
        // Arrange
        var httpClient = new HttpClient();
        IHttpContextAccessor? httpContextAccessorNulo = null;
        var correlationIdAccessor = new Mock<ICorrelationIdAccessor>().Object;

        // Act & Assert
        FluentActions.Invoking(() => new BaseExternalHttpClientTestavel(httpClient, httpContextAccessorNulo!, correlationIdAccessor))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("httpContextAccessor");
    }

    [Fact(DisplayName = "Não deve criar instância quando CorrelationIdAccessor for nulo")]
    [Trait("Infrastructure", "BaseExternalHttpClient")]
    public void Construtor_DeveLancarArgumentNullException_QuandoCorrelationIdAccessorNulo()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpContextAccessor = new Mock<IHttpContextAccessor>().Object;
        ICorrelationIdAccessor? correlationIdAccessorNulo = null;

        // Act & Assert
        FluentActions.Invoking(() => new BaseExternalHttpClientTestavel(httpClient, httpContextAccessor, correlationIdAccessorNulo!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("correlationIdAccessor");
    }

    #endregion
}
