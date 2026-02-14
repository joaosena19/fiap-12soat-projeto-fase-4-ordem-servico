using FluentAssertions;
using Infrastructure.Monitoramento;

namespace Tests.Infrastructure.Monitoramento
{
    public class NewRelicMetricsServiceTests
    {
        private readonly NewRelicMetricsService _sut;

        public NewRelicMetricsServiceTests()
        {
            _sut = new NewRelicMetricsService();
        }

        [Fact(DisplayName = "Deve registrar evento de criação de ordem de serviço sem exceção")]
        [Trait("Infrastructure", "NewRelicMetricsService")]
        public void RegistrarOrdemServicoCriada_NaoDeveLancarExcecao()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var usuarioId = Guid.NewGuid();

            // Act
            var acao = () => _sut.RegistrarOrdemServicoCriada(ordemServicoId, clienteId, usuarioId);

            // Assert
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "Deve registrar evento de mudança de status sem exceção")]
        [Trait("Infrastructure", "NewRelicMetricsService")]
        public void RegistrarMudancaOrdemServicoStatus_NaoDeveLancarExcecao()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var statusAnterior = "Criada";
            var statusNovo = "EmDiagnostico";
            var duracaoMs = 5000.0;

            // Act
            var acao = () => _sut.RegistrarMudancaOrdemServicoStatus(ordemServicoId, statusAnterior, statusNovo, duracaoMs);

            // Assert
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "Deve registrar compensação de saga por timeout sem exceção")]
        [Trait("Infrastructure", "NewRelicMetricsService")]
        public void RegistrarCompensacaoSagaTimeout_NaoDeveLancarExcecao()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var motivo = "timeout_estoque_indisponivel";
            var dataInicioExecucao = DateTime.UtcNow.AddMinutes(-5);

            // Act
            var acao = () => _sut.RegistrarCompensacaoSagaTimeout(ordemServicoId, motivo, dataInicioExecucao);

            // Assert
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "Deve registrar compensação de saga por timeout com data nula")]
        [Trait("Infrastructure", "NewRelicMetricsService")]
        public void RegistrarCompensacaoSagaTimeout_NaoDeveLancarExcecao_QuandoDataNula()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var motivo = "timeout_estoque_indisponivel";

            // Act
            var acao = () => _sut.RegistrarCompensacaoSagaTimeout(ordemServicoId, motivo, null);

            // Assert
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "Deve registrar estoque confirmado sem exceção")]
        [Trait("Infrastructure", "NewRelicMetricsService")]
        public void RegistrarEstoqueConfirmado_NaoDeveLancarExcecao()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var statusAtual = "EmExecucao";
            var correlationId = Guid.NewGuid().ToString();

            // Act
            var acao = () => _sut.RegistrarEstoqueConfirmado(ordemServicoId, statusAtual, correlationId);

            // Assert
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "Deve registrar compensação de falha de estoque sem exceção")]
        [Trait("Infrastructure", "NewRelicMetricsService")]
        public void RegistrarCompensacaoSagaFalhaEstoque_NaoDeveLancarExcecao()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var motivo = "estoque_insuficiente";
            var correlationId = Guid.NewGuid().ToString();

            // Act
            var acao = () => _sut.RegistrarCompensacaoSagaFalhaEstoque(ordemServicoId, motivo, correlationId);

            // Assert
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "Deve registrar falha crítica de compensação sem exceção")]
        [Trait("Infrastructure", "NewRelicMetricsService")]
        public void RegistrarCompensacaoSagaFalhaCritica_NaoDeveLancarExcecao()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var erro = "Erro ao atualizar banco de dados";
            var correlationId = Guid.NewGuid().ToString();

            // Act
            var acao = () => _sut.RegistrarCompensacaoSagaFalhaCritica(ordemServicoId, erro, correlationId);

            // Assert
            acao.Should().NotThrow();
        }
    }
}
