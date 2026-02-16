using Application.Contracts.Gateways;
using Application.Contracts.Monitoramento;
using FluentAssertions;
using Infrastructure.BackgroundServices;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Application.SharedHelpers.AggregateBuilders;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Infrastructure.BackgroundServices
{
    public class SagaTimeoutBackgroundServiceTests
    {
        private readonly Mock<IOrdemServicoGateway> _gatewayMock;
        private readonly Mock<IMetricsService> _metricsServiceMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly SagaTimeoutBackgroundService _sut;

        public SagaTimeoutBackgroundServiceTests()
        {
            _gatewayMock = new Mock<IOrdemServicoGateway>();
            _metricsServiceMock = new Mock<IMetricsService>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

            _sut = new SagaTimeoutBackgroundService(_gatewayMock.Object, _metricsServiceMock.Object, _loggerFactoryMock.Object);
        }

        [Fact(DisplayName = "Deve compensar ordens com timeout e registrar métricas")]
        [Trait("Infrastructure", "SagaTimeoutBackgroundService")]
        public async Task ExecuteAsync_DeveCompensarOrdensComTimeout_QuandoExistiremOrdensAguardando()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComItens()
                .ComServicos()
                .ComOrcamento(comItens: false)
                .Build();

            ordemServico.AprovarOrcamento();
            ordemServico.IniciarExecucao();

            _gatewayMock.Setup(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<OrdemServicoAggregate> { ordemServico });
            _gatewayMock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ReturnsAsync((OrdemServicoAggregate os) => os);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act
            await _sut.StartAsync(cts.Token);
            await Task.Delay(200);
            await _sut.StopAsync(CancellationToken.None);

            // Assert
            _gatewayMock.Verify(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
            _gatewayMock.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.AtLeastOnce);
            _metricsServiceMock.Verify(m => m.RegistrarCompensacaoSagaTimeout(ordemServico.Id, "timeout_estoque_indisponivel", It.IsAny<DateTime?>()), Times.AtLeastOnce);
        }

        [Fact(DisplayName = "Deve continuar executando quando não existirem ordens com timeout")]
        [Trait("Infrastructure", "SagaTimeoutBackgroundService")]
        public async Task ExecuteAsync_DeveContinuar_QuandoNaoExistiremOrdensComTimeout()
        {
            // Arrange
            _gatewayMock.Setup(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<OrdemServicoAggregate>());

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act
            await _sut.StartAsync(cts.Token);
            await Task.Delay(200);
            await _sut.StopAsync(CancellationToken.None);

            // Assert
            _gatewayMock.Verify(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
            _gatewayMock.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Never);
            _metricsServiceMock.Verify(m => m.RegistrarCompensacaoSagaTimeout(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [Fact(DisplayName = "Deve capturar exceção e continuar executando")]
        [Trait("Infrastructure", "SagaTimeoutBackgroundService")]
        public async Task ExecuteAsync_DeveCapturarExcecao_QuandoOcorrerErro()
        {
            // Arrange
            _gatewayMock.Setup(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>())).ThrowsAsync(new Exception("Erro de conexão"));

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act
            var acao = async () =>
            {
                await _sut.StartAsync(cts.Token);
                await Task.Delay(200);
                await _sut.StopAsync(CancellationToken.None);
            };

            // Assert
            await acao.Should().NotThrowAsync();
        }
    }
}
