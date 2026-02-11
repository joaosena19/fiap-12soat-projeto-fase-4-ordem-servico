using Application.Contracts.Gateways;
using Application.Contracts.Monitoramento;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Infrastructure.Monitoramento;

namespace Infrastructure.BackgroundServices;

public class SagaTimeoutBackgroundService : BackgroundService
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IMetricsService _metricsService;
    private readonly IAppLogger _logger;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan TimeoutThreshold = TimeSpan.FromSeconds(90);

    public SagaTimeoutBackgroundService(IOrdemServicoGateway gateway, IMetricsService metricsService, ILoggerFactory loggerFactory)
    {
        _gateway = gateway;
        _metricsService = metricsService;
        _logger = new LoggerAdapter<SagaTimeoutBackgroundService>(loggerFactory.CreateLogger<SagaTimeoutBackgroundService>());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var log = _logger.ComPropriedade("BackgroundService", nameof(SagaTimeoutBackgroundService));
        
        log.LogDebug("SagaTimeoutBackgroundService iniciado. Polling a cada {Interval}s, threshold de {Threshold}s.",
            PollingInterval.TotalSeconds, TimeoutThreshold.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarOrdensComTimeoutAsync(log);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Erro no SagaTimeoutBackgroundService ao verificar ordens com timeout.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task VerificarOrdensComTimeoutAsync(IAppLogger log)
    {
        var timeoutLimit = DateTime.UtcNow.Subtract(TimeoutThreshold);
        var ordensComTimeout = await _gateway.ObterOrdensAguardandoEstoqueComTimeoutAsync(timeoutLimit);

        foreach (var os in ordensComTimeout)
        {
            os.RegistrarFalhaReducaoEstoque();
            await _gateway.AtualizarAsync(os);

            _metricsService.RegistrarCompensacaoSagaTimeout(
                os.Id,
                "timeout_estoque_indisponivel",
                os.Historico.DataInicioExecucao);

            log.LogWarning(
                "Compensação por timeout aplicada para Ordem Serviço {OsId}. Status revertido para Aprovada. Motivo: timeout_estoque_indisponivel",
                os.Id);
        }
    }
}
