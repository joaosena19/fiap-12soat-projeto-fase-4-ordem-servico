using Application.Contracts.Gateways;
using Application.Contracts.Monitoramento;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundServices;

public class SagaTimeoutBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger _logger;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan TimeoutThreshold = TimeSpan.FromSeconds(90);

    public SagaTimeoutBackgroundService(IServiceProvider serviceProvider, IAppLogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SagaTimeoutBackgroundService iniciado. Polling a cada {Interval}s, threshold de {Threshold}s.",
            PollingInterval.TotalSeconds, TimeoutThreshold.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarOrdensComTimeoutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no SagaTimeoutBackgroundService ao verificar ordens com timeout.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task VerificarOrdensComTimeoutAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        var metrics = scope.ServiceProvider.GetRequiredService<IMetricsService>();

        var timeoutLimit = DateTime.UtcNow.Subtract(TimeoutThreshold);
        var ordensComTimeout = await gateway.ObterOrdensAguardandoEstoqueComTimeoutAsync(timeoutLimit);

        foreach (var os in ordensComTimeout)
        {
            os.CompensarFalhaSaga();
            await gateway.AtualizarAsync(os);

            metrics.RegistrarCompensacaoSagaTimeout(
                os.Id,
                "timeout_estoque_indisponivel",
                os.Historico.DataInicioExecucao);

            _logger.LogWarning(
                "Compensação por timeout aplicada para OS {OsId}. Status revertido para Aprovada. Motivo: timeout_estoque_indisponivel",
                os.Id);
        }
    }
}
