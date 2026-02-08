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

        public void RegistrarEstoqueConfirmado(Guid ordemServicoId, string statusAtual, Guid correlationId)
        {
            var atributos = new Dictionary<string, object>
            {
                { "ordemServicoId", ordemServicoId },
                { "statusAtual", statusAtual },
                { "correlationId", correlationId },
                { "timestamp", DateTime.UtcNow }
            };

            NR.NewRelic.RecordCustomEvent("SagaEstoqueConfirmado", atributos);
        }

        public void RegistrarCompensacaoSagaFalhaEstoque(Guid ordemServicoId, string motivo, Guid correlationId)
        {
            var atributos = new Dictionary<string, object>
            {
                { "ordemServicoId", ordemServicoId },
                { "motivo", motivo },
                { "correlationId", correlationId },
                { "timestamp", DateTime.UtcNow }
            };

            NR.NewRelic.RecordCustomEvent("SagaCompensacaoFalhaEstoque", atributos);
        }

        public void RegistrarCompensacaoSagaFalhaCritica(Guid ordemServicoId, string erro, Guid correlationId)
        {
            var atributos = new Dictionary<string, object>
            {
                { "ordemServicoId", ordemServicoId },
                { "erro", erro },
                { "correlationId", correlationId },
                { "timestamp", DateTime.UtcNow }
            };

            NR.NewRelic.RecordCustomEvent("SagaCompensacaoFalhaCritica", atributos);
        }
    }
}