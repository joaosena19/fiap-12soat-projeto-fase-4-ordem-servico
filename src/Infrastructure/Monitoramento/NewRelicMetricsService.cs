using Application.Contracts.Monitoramento;
using NR = NewRelic.Api.Agent;

namespace Infrastructure.Monitoramento
{
    public class NewRelicMetricsService : IMetricsService
    {
        public void RegistrarOrdemServicoCriada(Guid ordemServicoId, Guid clienteId, Guid usuarioId)
        {
            var atributos = new Dictionary<string, object>
            {
                { "OrdemServicoId", ordemServicoId },
                { "ClienteId", clienteId },
                { "UsuarioId", usuarioId }
            };

            NR.NewRelic.RecordCustomEvent("OrdemServicoCriada", atributos);
        }

        public void RegistrarMudancaOrdemServicoStatus(Guid ordemServicoId, string statusAnterior, string statusNovo, double duracaoMs)
        {
            var atributos = new Dictionary<string, object>
            {
                { "OrdemServicoId", ordemServicoId },
                { "StatusAnterior", statusAnterior },
                { "StatusNovo", statusNovo },
                { "DuracaoMs", duracaoMs }
            };

            NR.NewRelic.RecordCustomEvent("OrdemServicoMudancaStatus", atributos);
        }

        public void RegistrarCompensacaoSagaTimeout(Guid ordemServicoId, string motivo, DateTime? dataInicioExecucao)
        {
            var atributos = new Dictionary<string, object>
            {
                { "ordemServicoId", ordemServicoId },
                { "motivo", motivo },
                { "dataInicioExecucao", dataInicioExecucao?.ToString("o") ?? "N/A" },
                { "sucesso", true }
            };

            NR.NewRelic.RecordCustomEvent("SagaCompensacaoTimeout", atributos);
        }
    }
}