using Application.Contracts.Gateways;
using Application.Contracts.Messaging.DTOs;
using Application.Contracts.Monitoramento;
using Domain.OrdemServico.Enums;
using FluentAssertions;
using Infrastructure.Messaging;
using MassTransit;
using Moq;
using Tests.Application.SharedHelpers;
using Tests.Application.SharedHelpers.AggregateBuilders;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Infrastructure.Messaging
{
    public class ReducaoEstoqueResultadoConsumerTests
    {
        private readonly Mock<IOrdemServicoGateway> _gatewayMock;
        private readonly MockLogger _mockLogger;
        private readonly Mock<IMetricsService> _metricsServiceMock;
        private readonly ReducaoEstoqueResultadoConsumer _sut;

        public ReducaoEstoqueResultadoConsumerTests()
        {
            _gatewayMock = new Mock<IOrdemServicoGateway>();
            _mockLogger = MockLogger.Criar();
            _metricsServiceMock = new Mock<IMetricsService>();
            _sut = new ReducaoEstoqueResultadoConsumer(_gatewayMock.Object, _mockLogger.Object, _metricsServiceMock.Object);
        }

        private static Mock<ConsumeContext<ReducaoEstoqueResultado>> CriarConsumeContext(ReducaoEstoqueResultado mensagem)
        {
            var contextMock = new Mock<ConsumeContext<ReducaoEstoqueResultado>>();
            contextMock.Setup(c => c.Message).Returns(mensagem);
            return contextMock;
        }

        #region Cenarios de Sucesso

        [Fact(DisplayName = "Deve confirmar redução de estoque quando mensagem é de sucesso")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveConfirmarReducao_QuandoMensagemSucesso()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComItens()
                .ComServicos()
                .ComOrcamento(comItens: false)
                .Build();

            ordemServico.AprovarOrcamento();
            ordemServico.IniciarExecucao();

            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = ordemServico.Id,
                Sucesso = true,
                MotivoFalha = null
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(ordemServico.Id)).ReturnsAsync(ordemServico);
            _gatewayMock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ReturnsAsync((OrdemServicoAggregate os) => os);

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _gatewayMock.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Once);
            _metricsServiceMock.Verify(m => m.RegistrarEstoqueConfirmado(ordemServico.Id, It.IsAny<string>(), mensagem.CorrelationId), Times.Once);
        }

        #endregion

        #region Cenarios de Falha

        [Fact(DisplayName = "Deve executar compensação quando mensagem é de falha")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveExecutarCompensacao_QuandoMensagemFalha()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComItens()
                .ComServicos()
                .ComOrcamento(comItens: false)
                .Build();

            ordemServico.AprovarOrcamento();
            ordemServico.IniciarExecucao();

            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = ordemServico.Id,
                Sucesso = false,
                MotivoFalha = "estoque_insuficiente"
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(ordemServico.Id)).ReturnsAsync(ordemServico);
            _gatewayMock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ReturnsAsync((OrdemServicoAggregate os) => os);

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _gatewayMock.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Once);
            _metricsServiceMock.Verify(m => m.RegistrarCompensacaoSagaFalhaEstoque(ordemServico.Id, "estoque_insuficiente", mensagem.CorrelationId), Times.Once);
        }

        [Fact(DisplayName = "Deve usar motivo desconhecido quando MotivoFalha é nulo")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveUsarMotivoDesconhecido_QuandoMotivoFalhaNulo()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComItens()
                .ComServicos()
                .ComOrcamento(comItens: false)
                .Build();

            ordemServico.AprovarOrcamento();
            ordemServico.IniciarExecucao();

            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = ordemServico.Id,
                Sucesso = false,
                MotivoFalha = null
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(ordemServico.Id)).ReturnsAsync(ordemServico);
            _gatewayMock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ReturnsAsync((OrdemServicoAggregate os) => os);

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _metricsServiceMock.Verify(m => m.RegistrarCompensacaoSagaFalhaEstoque(ordemServico.Id, "desconhecido", mensagem.CorrelationId), Times.Once);
        }

        #endregion

        #region Salvaguardas

        [Fact(DisplayName = "Deve ignorar quando ordem de serviço não encontrada")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveIgnorar_QuandoOrdemServicoNaoEncontrada()
        {
            // Arrange
            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = Guid.NewGuid(),
                Sucesso = true
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(mensagem.OrdemServicoId)).ReturnsAsync((OrdemServicoAggregate?)null);

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _gatewayMock.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Never);
        }

        [Fact(DisplayName = "Deve ignorar quando ordem de serviço não está configurada para redução de estoque")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveIgnorar_QuandoOrdemNaoConfiguradaParaReducao()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComServicos()
                .Build();

            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = ordemServico.Id,
                Sucesso = true
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(ordemServico.Id)).ReturnsAsync(ordemServico);

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _gatewayMock.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Never);
        }

        [Fact(DisplayName = "Deve ignorar quando estoque já foi removido com sucesso")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveIgnorar_QuandoEstoqueJaRemovidoComSucesso()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComItens()
                .ComServicos()
                .ProntoParaFinalizacao()
                .Build();

            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = ordemServico.Id,
                Sucesso = true
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(ordemServico.Id)).ReturnsAsync(ordemServico);

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _gatewayMock.Verify(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Never);
        }

        #endregion

        #region Tratamento de Exceções

        [Fact(DisplayName = "Deve logar erro inesperado no Consume sem propagar exceção")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveLogarErro_QuandoExcecaoInesperada()
        {
            // Arrange
            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = Guid.NewGuid(),
                Sucesso = true
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(mensagem.OrdemServicoId)).ThrowsAsync(new Exception("Erro de conexão"));

            var context = CriarConsumeContext(mensagem);

            // Act
            var acao = async () => await _sut.Consume(context.Object);

            // Assert
            await acao.Should().NotThrowAsync();
        }

        [Fact(DisplayName = "Deve registrar falha crítica quando compensação falha")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveRegistrarFalhaCritica_QuandoCompensacaoFalha()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComItens()
                .ComServicos()
                .ComOrcamento(comItens: false)
                .Build();

            ordemServico.AprovarOrcamento();
            ordemServico.IniciarExecucao();

            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = ordemServico.Id,
                Sucesso = false,
                MotivoFalha = "erro_interno"
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(ordemServico.Id)).ReturnsAsync(ordemServico);
            _gatewayMock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ThrowsAsync(new Exception("Erro ao salvar"));

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _metricsServiceMock.Verify(m => m.RegistrarCompensacaoSagaFalhaCritica(mensagem.OrdemServicoId, It.IsAny<string>(), mensagem.CorrelationId), Times.Once);
        }

        [Fact(DisplayName = "Deve registrar falha crítica quando confirmação de sucesso falha")]
        [Trait("Infrastructure", "ReducaoEstoqueResultadoConsumer")]
        public async Task Consume_DeveRegistrarFalhaCritica_QuandoConfirmacaoSucessoFalha()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder()
                .ComItens()
                .ComServicos()
                .ComOrcamento(comItens: false)
                .Build();

            ordemServico.AprovarOrcamento();
            ordemServico.IniciarExecucao();

            var mensagem = new ReducaoEstoqueResultado
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = ordemServico.Id,
                Sucesso = true
            };

            _gatewayMock.Setup(g => g.ObterPorIdAsync(ordemServico.Id)).ReturnsAsync(ordemServico);
            _gatewayMock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ThrowsAsync(new Exception("Erro ao salvar"));

            var context = CriarConsumeContext(mensagem);

            // Act
            await _sut.Consume(context.Object);

            // Assert
            _metricsServiceMock.Verify(m => m.RegistrarCompensacaoSagaFalhaCritica(ordemServico.Id, It.IsAny<string>(), mensagem.CorrelationId), Times.Once);
        }

        #endregion
    }
}
