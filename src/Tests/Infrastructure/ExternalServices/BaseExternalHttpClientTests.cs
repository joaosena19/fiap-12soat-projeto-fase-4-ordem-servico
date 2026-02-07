using Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net.Http.Headers;
using Xunit;

namespace Tests.Infrastructure.ExternalServices;

/// <summary>
/// Testes unitários para BaseExternalHttpClient.
/// </summary>
public class BaseExternalHttpClientTests
{
    private class TestExternalHttpClient : BaseExternalHttpClient
    {
        public TestExternalHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public void TestPropagateAuthToken() => PropagateAuthToken();
        public void TestPropagateCorrelationId() => PropagateCorrelationId();
        public void TestPropagateHeaders() => PropagateHeaders();
    }

    [Fact]
    public void PropagateAuthToken_WhenAuthHeaderExists_SetsAuthorizationHeader()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer test-token-123";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var client = new TestExternalHttpClient(httpClient, httpContextAccessorMock.Object);

        // Act
        client.TestPropagateAuthToken();

        // Assert
        Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        Assert.Equal("test-token-123", httpClient.DefaultRequestHeaders.Authorization.Parameter);
    }

    [Fact]
    public void PropagateAuthToken_WhenNoAuthHeader_DoesNotSetHeader()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        // Não definir Authorization header
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var client = new TestExternalHttpClient(httpClient, httpContextAccessorMock.Object);

        // Act
        client.TestPropagateAuthToken();

        // Assert
        Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
    }

    [Fact]
    public void PropagateCorrelationId_WhenCorrelationIdExists_AddsHeaderToRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-ID"] = "correlation-123";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var client = new TestExternalHttpClient(httpClient, httpContextAccessorMock.Object);

        // Act
        client.TestPropagateCorrelationId();

        // Assert
        Assert.True(httpClient.DefaultRequestHeaders.Contains("X-Correlation-ID"));
        var correlationId = httpClient.DefaultRequestHeaders.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal("correlation-123", correlationId);
    }

    [Fact]
    public void PropagateCorrelationId_WhenNoCorrelationId_GeneratesNewOne()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        // Não definir X-Correlation-ID header
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var client = new TestExternalHttpClient(httpClient, httpContextAccessorMock.Object);

        // Act
        client.TestPropagateCorrelationId();

        // Assert
        Assert.True(httpClient.DefaultRequestHeaders.Contains("X-Correlation-ID"));
        var correlationId = httpClient.DefaultRequestHeaders.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.NotNull(correlationId);
        Assert.True(Guid.TryParse(correlationId, out _)); // Valida que é um GUID
    }

    [Fact]
    public void PropagateHeaders_PropagatesBothAuthTokenAndCorrelationId()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer test-token-456";
        httpContext.Request.Headers["X-Correlation-ID"] = "correlation-456";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var client = new TestExternalHttpClient(httpClient, httpContextAccessorMock.Object);

        // Act
        client.TestPropagateHeaders();

        // Assert
        Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        Assert.Equal("test-token-456", httpClient.DefaultRequestHeaders.Authorization.Parameter);
        
        Assert.True(httpClient.DefaultRequestHeaders.Contains("X-Correlation-ID"));
        var correlationId = httpClient.DefaultRequestHeaders.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal("correlation-456", correlationId);
    }

    [Fact]
    public void Constructor_WhenHttpClientIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new TestExternalHttpClient(null!, httpContextAccessorMock.Object));
    }

    [Fact]
    public void Constructor_WhenHttpContextAccessorIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new TestExternalHttpClient(httpClient, null!));
    }

    [Fact]
    public void PropagateCorrelationId_WhenCalledMultipleTimes_ReplacesExistingHeader()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-ID"] = "correlation-first";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var client = new TestExternalHttpClient(httpClient, httpContextAccessorMock.Object);

        // Act - First call
        client.TestPropagateCorrelationId();
        var firstCorrelationId = httpClient.DefaultRequestHeaders.GetValues("X-Correlation-ID").FirstOrDefault();

        // Change the header value
        httpContext.Request.Headers["X-Correlation-ID"] = "correlation-second";

        // Act - Second call
        client.TestPropagateCorrelationId();
        var secondCorrelationId = httpClient.DefaultRequestHeaders.GetValues("X-Correlation-ID").FirstOrDefault();

        // Assert
        Assert.Equal("correlation-first", firstCorrelationId);
        Assert.Equal("correlation-second", secondCorrelationId);
        // Verify only one header exists
        Assert.Single(httpClient.DefaultRequestHeaders.GetValues("X-Correlation-ID"));
    }
}
