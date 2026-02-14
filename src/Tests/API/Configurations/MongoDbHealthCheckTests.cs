using API.Configurations;
using FluentAssertions;
using Infrastructure.Database;
using Infrastructure.Repositories.OrdemServico;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Moq;

namespace Tests.API.Configurations
{
    public class MongoDbHealthCheckTests
    {
        [Fact(DisplayName = "CheckHealthAsync deve retornar Healthy quando MongoDB responder")]
        [Trait("API", "MongoDbHealthCheck")]
        public async Task CheckHealthAsync_DeveRetornarHealthy_QuandoMongoResponder()
        {
            // Arrange
            var collectionMock = new Mock<IMongoCollection<OrdemServicoDocument>>();
            collectionMock
                .Setup(c => c.EstimatedDocumentCountAsync(It.IsAny<EstimatedDocumentCountOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(10L);

            var contextMock = new Mock<MongoDbContext>();
            contextMock.Setup(c => c.OrdensServico).Returns(collectionMock.Object);

            var healthCheck = new MongoDbHealthCheck(contextMock.Object);
            var healthCheckContext = new HealthCheckContext();

            // Act
            var resultado = await healthCheck.CheckHealthAsync(healthCheckContext);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Status.Should().Be(HealthStatus.Healthy);
            resultado.Description.Should().Be("MongoDB está acessível.");
            resultado.Exception.Should().BeNull();
        }

        [Fact(DisplayName = "CheckHealthAsync deve retornar Unhealthy quando MongoDB lançar exceção")]
        [Trait("API", "MongoDbHealthCheck")]
        public async Task CheckHealthAsync_DeveRetornarUnhealthy_QuandoMongoLancarExcecao()
        {
            // Arrange
            var excecaoEsperada = new MongoException("Erro de conexão");
            var collectionMock = new Mock<IMongoCollection<OrdemServicoDocument>>();
            collectionMock
                .Setup(c => c.EstimatedDocumentCountAsync(It.IsAny<EstimatedDocumentCountOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(excecaoEsperada);

            var contextMock = new Mock<MongoDbContext>();
            contextMock.Setup(c => c.OrdensServico).Returns(collectionMock.Object);

            var healthCheck = new MongoDbHealthCheck(contextMock.Object);
            var healthCheckContext = new HealthCheckContext();

            // Act
            var resultado = await healthCheck.CheckHealthAsync(healthCheckContext);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Status.Should().Be(HealthStatus.Unhealthy);
            resultado.Description.Should().Be("MongoDB não está acessível.");
            resultado.Exception.Should().Be(excecaoEsperada);
        }

        [Fact(DisplayName = "CheckHealthAsync deve usar CancellationToken passado como parâmetro")]
        [Trait("API", "MongoDbHealthCheck")]
        public async Task CheckHealthAsync_DeveUsarCancellationToken_PassadoComoParametro()
        {
            // Arrange
            var cancellationTokenCapturado = default(CancellationToken);
            var collectionMock = new Mock<IMongoCollection<OrdemServicoDocument>>();
            collectionMock
                .Setup(c => c.EstimatedDocumentCountAsync(It.IsAny<EstimatedDocumentCountOptions>(), It.IsAny<CancellationToken>()))
                .Callback<EstimatedDocumentCountOptions, CancellationToken>((_, token) => cancellationTokenCapturado = token)
                .ReturnsAsync(5L);

            var contextMock = new Mock<MongoDbContext>();
            contextMock.Setup(c => c.OrdensServico).Returns(collectionMock.Object);

            var healthCheck = new MongoDbHealthCheck(contextMock.Object);
            var healthCheckContext = new HealthCheckContext();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            await healthCheck.CheckHealthAsync(healthCheckContext, cancellationToken);

            // Assert
            cancellationTokenCapturado.Should().Be(cancellationToken, "o cancellation token deve ser propagado para a operação do MongoDB");
        }

        [Fact(DisplayName = "CheckHealthAsync deve funcionar com CancellationToken default")]
        [Trait("API", "MongoDbHealthCheck")]
        public async Task CheckHealthAsync_DeveFuncionar_ComCancellationTokenDefault()
        {
            // Arrange
            var collectionMock = new Mock<IMongoCollection<OrdemServicoDocument>>();
            collectionMock
                .Setup(c => c.EstimatedDocumentCountAsync(It.IsAny<EstimatedDocumentCountOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(3L);

            var contextMock = new Mock<MongoDbContext>();
            contextMock.Setup(c => c.OrdensServico).Returns(collectionMock.Object);

            var healthCheck = new MongoDbHealthCheck(contextMock.Object);
            var healthCheckContext = new HealthCheckContext();

            // Act
            var resultado = await healthCheck.CheckHealthAsync(healthCheckContext);

            // Assert
            resultado.Status.Should().Be(HealthStatus.Healthy);
            collectionMock.Verify(c => c.EstimatedDocumentCountAsync(It.IsAny<EstimatedDocumentCountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
