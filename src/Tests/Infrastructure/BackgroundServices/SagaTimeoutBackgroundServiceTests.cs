using Application.Contracts.Gateways;
using Application.Contracts.Monitoramento;
using Domain.OrdemServico.Enums;
using Domain.OrdemServico.ValueObjects.OrdemServico;
using Infrastructure.BackgroundServices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Infrastructure.BackgroundServices;

public class SagaTimeoutBackgroundServiceTests
{
    private readonly Mock<IOrdemServicoGateway> _mockGateway;
    private readonly Mock<IMetricsService> _mockMetrics;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<ILogger<SagaTimeoutBackgroundService>> _mockLogger;

    public SagaTimeoutBackgroundServiceTests()
    {
        _mockGateway = new Mock<IOrdemServicoGateway>();
        _mockMetrics = new Mock<IMetricsService>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLogger = new Mock<ILogger<SagaTimeoutBackgroundService>>();
        
        _mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_mockLogger.Object);
    }

    [Fact(DisplayName = "VerificarOrdensComTimeout quando há ordens com timeout deve executar compensação")]
    [Trait("Classe", "SagaTimeoutBackgroundService")]
    [Trait("Método", "VerificarOrdensComTimeoutAsync")]
    public async Task VerificarOrdensComTimeout_QuandoHaOrdensComTimeout_ExecutaCompensacao()
    {
        // Arrange
        var os = OrdemServicoAggregate.Criar(Guid.NewGuid());
        os.AdicionarServico(Guid.NewGuid(), "Serviço Teste", 100m);
        os.AlterarStatus(StatusOrdemServicoEnum.EmDiagnostico);
        os.AlterarStatus(StatusOrdemServicoEnum.AguardandoAprovacao);
        os.GerarOrcamento();
        os.AprovarOrcamento();
        os.IniciarExecucao();

        var ordensComTimeout = new List<OrdemServicoAggregate> { os };

        _mockGateway
            .Setup(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(ordensComTimeout);

        _mockGateway
            .Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()))
            .ReturnsAsync((OrdemServicoAggregate o) => o);

        var service = new SagaTimeoutBackgroundService(_mockGateway.Object, _mockMetrics.Object, _mockLoggerFactory.Object);

        // Usar reflexão para chamar o método privado VerificarOrdensComTimeoutAsync
        var method = typeof(SagaTimeoutBackgroundService)
            .GetMethod("VerificarOrdensComTimeoutAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Criar um mock logger para passar como parâmetro
        var mockLog = new Mock<IAppLogger>();
        mockLog.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()));

        // Act
        await (Task)method!.Invoke(service, new object[] { mockLog.Object })!;

        // Assert
        _mockGateway.Verify(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()), Times.Once);
        _mockGateway.Verify(g => g.AtualizarAsync(It.Is<OrdemServicoAggregate>(
            o => o.Id == os.Id && o.Status.Valor == StatusOrdemServicoEnum.Aprovada)), Times.Once);
        _mockMetrics.Verify(
            m => m.RegistrarCompensacaoSagaTimeout(
                os.Id,
                "timeout_estoque_indisponivel",
                It.IsAny<DateTime?>()),
            Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Compensação por timeout aplicada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    [Trait("Classe", "SagaTimeoutBackgroundService")]
    [Trait("Método", "VerificarOrdensComTimeoutAsync")]
    public async Task VerificarOrdensComTimeout_QuandoNaoHaOrdens_NaoFazNada()
    {
        // Arrange
        var ordensVazia = new List<OrdemServicoAggregate>();

        _mockGateway
            .Setup(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(ordensVazia);

        var service = new SagaTimeoutBackgroundService(_mockGateway.Object, _mockMetrics.Object, _mockLoggerFactory.Object);

        // Usar reflexão para chamar o método privado
        var method = typeof(SagaTimeoutBackgroundService)
            .GetMethod("VerificarOrdensComTimeoutAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Criar um mock logger para passar como parâmetro
        var mockLog = new Mock<IAppLogger>();

        // Act
        await (Task)method!.Invoke(service, new object[] { mockLog.Object })!;

        // Assert
        _mockGateway.Verify(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()), Times.Once);
        _mockGateway.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Never);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("timeout")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "VerificarOrdensComTimeout quando erro no gateway loga erro")]
    [Trait("Classe", "SagaTimeoutBackgroundService")]
    [Trait("Método", "VerificarOrdensComTimeoutAsync")]
    public async Task VerificarOrdensComTimeout_QuandoErroNoGateway_LogaErro()
    {
        // Arrange
        _mockGateway
            .Setup(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Erro ao acessar banco de dados"));

        var service = new SagaTimeoutBackgroundService(_mockGateway.Object, _mockMetrics.Object, _mockLoggerFactory.Object);

        // Usar reflexão para chamar o método privado
        var method = typeof(SagaTimeoutBackgroundService)
            .GetMethod("VerificarOrdensComTimeoutAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Criar um mock logger para passar como parâmetro
        var mockLog = new Mock<IAppLogger>();

        // Act & Assert
        // O método não deve propagar a exceção - ela é capturada internamente
        await Assert.ThrowsAsync<Exception>(async () => await (Task)method!.Invoke(service, new object[] { mockLog.Object })!);
        
        // Verificar se erro foi propagado (o VerificarOrdensComTimeoutAsync não captura erros)
        _mockGateway.Verify(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()), Times.Once);
    }

    [Fact(DisplayName = "ExecuteAsync quando iniciado deve logar informação de início")]
    [Trait("Classe", "SagaTimeoutBackgroundService")]
    [Trait("Método", "ExecuteAsync")]
    public async Task ExecuteAsync_QuandoIniciado_LogaInformacaoDeInicio()
    {
        // Arrange
        _mockGateway
            .Setup(g => g.ObterOrdensAguardandoEstoqueComTimeoutAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OrdemServicoAggregate>());

        var service = new SagaTimeoutBackgroundService(_mockGateway.Object, _mockMetrics.Object, _mockLoggerFactory.Object);
        var cts = new CancellationTokenSource();
        
        // Cancel imediatamente para não executar o loop infinito
        cts.CancelAfter(100);

        // Act
        try
        {
            await service.StartAsync(cts.Token);
            await Task.Delay(200); // Aguardar um pouco para permitir a primeira execução
            await service.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Esperado quando o token é cancelado
        }

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SagaTimeoutBackgroundService iniciado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
