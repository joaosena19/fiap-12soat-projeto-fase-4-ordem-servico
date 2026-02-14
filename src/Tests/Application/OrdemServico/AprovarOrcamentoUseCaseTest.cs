using Application.Contracts.Presenters;
using Application.Contracts.Messaging;
using Application.Contracts.Messaging.DTOs;
using FluentAssertions;
using Moq;
using Shared.Enums;
using Tests.Application.OrdemServico.Helpers;
using Tests.Helpers;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;
using Domain.OrdemServico.Enums;
using Tests.Application.SharedHelpers;
using Tests.Application.SharedHelpers.Gateways;
using Tests.Application.SharedHelpers.ExternalServices;
using Tests.Application.SharedHelpers.AggregateBuilders;

namespace Tests.Application.OrdemServico
{
    public class AprovarOrcamentoUseCaseTest
    {
        private readonly OrdemServicoTestFixture _fixture;

        public AprovarOrcamentoUseCaseTest()
        {
            _fixture = new OrdemServicoTestFixture();
        }

        [Fact(DisplayName = "Deve executar primeira aprovação com itens - publica mensagem SQS")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_PrimeiraAprovacao_ComItens_PublicaMensagem()
        {
            // Arrange
            var ator = new AtorBuilder().ComoAdministrador().Build();
            var ordemServico = new OrdemServicoBuilder().ComOrcamento().Build();

            OrdemServicoAggregate? ordemServicoAtualizada = null;

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServico.Id).Retorna(ordemServico);
            _fixture.VeiculoExternalServiceMock.AoObterPorId(ordemServico.VeiculoId).Retorna(new VeiculoExternalDtoBuilder().Build());
            _fixture.OrdemServicoGatewayMock.AoAtualizar().ComCallback(os => ordemServicoAtualizada = os);

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                ator,
                ordemServico.Id,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            ordemServicoAtualizada.Should().NotBeNull();
            ordemServicoAtualizada!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
            ordemServicoAtualizada.InteracaoEstoque.DeveRemoverEstoque.Should().BeTrue();
            ordemServicoAtualizada.InteracaoEstoque.EstaAguardandoRemocaoEstoque.Should().BeTrue();

            _fixture.EstoqueMessagePublisherMock.Verify(x => x.PublicarSolicitacaoReducaoAsync(
                It.Is<ReducaoEstoqueSolicitacao>(s => 
                    s.OrdemServicoId == ordemServico.Id && 
                    s.Itens.Count == ordemServico.ItensIncluidos.Count())), Times.Once);

            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoSucesso();
            _fixture.OperacaoOrdemServicoPresenterMock.NaoDeveTerApresentadoErro();
        }

        [Fact(DisplayName = "Deve executar retry pós-compensação com itens - publica mensagem SQS")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_RetryPosCompensacao_ComItens_PublicaMensagem()
        {
            // Arrange
            var ator = new AtorBuilder().ComoAdministrador().Build();
            var ordemServico = new OrdemServicoBuilder().ComOrcamento().Build();
            
            // Simular OS que foi compensada (voltou para Aprovada)
            ordemServico.AprovarOrcamento(); // AguardandoAprovacao → Aprovada
            OrdemServicoAggregate? ordemServicoAtualizada = null;

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServico.Id).Retorna(ordemServico);
            _fixture.VeiculoExternalServiceMock.AoObterPorId(ordemServico.VeiculoId).Retorna(new VeiculoExternalDtoBuilder().Build());
            _fixture.OrdemServicoGatewayMock.AoAtualizar().ComCallback(os => ordemServicoAtualizada = os);

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                ator,
                ordemServico.Id,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            ordemServicoAtualizada.Should().NotBeNull();
            ordemServicoAtualizada!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);

            _fixture.EstoqueMessagePublisherMock.Verify(x => x.PublicarSolicitacaoReducaoAsync(
                It.IsAny<ReducaoEstoqueSolicitacao>()), Times.Once);

            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoSucesso();
        }

        [Fact(DisplayName = "Deve executar retry quando estoque já foi confirmado - não publica mensagem")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_RetryComEstoqueJaConfirmado_NaoPublicaMensagem()
        {
            // Arrange
            var ator = new AtorBuilder().ComoAdministrador().Build();
            var ordemServico = new OrdemServicoBuilder().ComOrcamento().Build();
            
            // Simular OS em estado: Aprovada + estoque já confirmado de tentativa anterior
            ordemServico.AprovarOrcamento(); // → Aprovada
            ordemServico.IniciarExecucao(); // → EmExecucao + aguardando estoque
            ordemServico.ConfirmarReducaoEstoque(); // estoque confirmado (EstoqueRemovidoComSucesso = true)
            // Reverter status para Aprovada via reflection (simulando retry sem alterar InteracaoEstoque)
            var statusProperty = ordemServico.GetType().GetProperty("Status");
            statusProperty!.SetValue(ordemServico, new global::Domain.OrdemServico.ValueObjects.OrdemServico.Status(StatusOrdemServicoEnum.Aprovada));

            OrdemServicoAggregate? ordemServicoAtualizada = null;

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServico.Id).Retorna(ordemServico);
            _fixture.VeiculoExternalServiceMock.AoObterPorId(ordemServico.VeiculoId).Retorna(new VeiculoExternalDtoBuilder().Build());
            _fixture.OrdemServicoGatewayMock.AoAtualizar().ComCallback(os => ordemServicoAtualizada = os);

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                ator,
                ordemServico.Id,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            ordemServicoAtualizada.Should().NotBeNull();
            ordemServicoAtualizada!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);

            // NÃO deve publicar mensagem pois estoque já foi confirmado
            _fixture.EstoqueMessagePublisherMock.Verify(x => x.PublicarSolicitacaoReducaoAsync(
                It.IsAny<ReducaoEstoqueSolicitacao>()), Times.Never);

            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoSucesso();
        }

        [Fact(DisplayName = "Deve executar sem itens (apenas serviços) - não publica mensagem")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_SemItens_ApenasServicos_NaoPublicaMensagem()
        {
            // Arrange
            var ator = new AtorBuilder().ComoAdministrador().Build();
            var ordemServico = new OrdemServicoBuilder().ComServicos().ComOrcamento(comItens: false).Build();

            OrdemServicoAggregate? ordemServicoAtualizada = null;

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServico.Id).Retorna(ordemServico);
            _fixture.VeiculoExternalServiceMock.AoObterPorId(ordemServico.VeiculoId).Retorna(new VeiculoExternalDtoBuilder().Build());
            _fixture.OrdemServicoGatewayMock.AoAtualizar().ComCallback(os => ordemServicoAtualizada = os);

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                ator,
                ordemServico.Id,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            ordemServicoAtualizada.Should().NotBeNull();
            ordemServicoAtualizada!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
            ordemServicoAtualizada.InteracaoEstoque.DeveRemoverEstoque.Should().BeFalse();
            ordemServicoAtualizada.InteracaoEstoque.SemPendenciasEstoque.Should().BeTrue();

            // Não deve publicar mensagem pois não há itens de estoque
            _fixture.EstoqueMessagePublisherMock.Verify(x => x.PublicarSolicitacaoReducaoAsync(
                It.IsAny<ReducaoEstoqueSolicitacao>()), Times.Never);

            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoSucesso();
        }

        [Fact(DisplayName = "Deve apresentar erro quando ordem de serviço não existir")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_QuandoOrdemServicoNaoExistir_ApresentaErroResourceNotFound()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServicoId).NaoRetornaNada();

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                new AtorBuilder().ComoAdministrador().Build(),
                ordemServicoId,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoErro("Ordem de serviço não encontrada.", ErrorType.ResourceNotFound);
            _fixture.OperacaoOrdemServicoPresenterMock.NaoDeveTerApresentadoSucesso();
        }

        [Fact(DisplayName = "Deve apresentar erro quando status for inválido")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_QuandoStatusInvalido_ApresentaErroDominio()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder().ComStatus(StatusOrdemServicoEnum.EmExecucao).Build();
            // OS em EmExecução não pode ser aprovada (deve estar em AguardandoAprovacao ou Aprovada)

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServico.Id).Retorna(ordemServico);
            _fixture.VeiculoExternalServiceMock.AoObterPorId(ordemServico.VeiculoId).Retorna(new VeiculoExternalDtoBuilder().Build());

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                new AtorBuilder().ComoAdministrador().Build(),
                ordemServico.Id,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoErro(
                $"Só é possível aprovar orçamento para uma OS com status '{StatusOrdemServicoEnum.AguardandoAprovacao}' ou '{StatusOrdemServicoEnum.Aprovada}'.", 
                ErrorType.DomainRuleBroken);
            _fixture.OperacaoOrdemServicoPresenterMock.NaoDeveTerApresentadoSucesso();
        }



        [Fact(DisplayName = "Deve apresentar erro quando cliente tenta aprovar orçamento de outro cliente")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_QuandoClienteTentaAprovarOrcamentoDeOutroCliente_ApresentaErroNotAllowed()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var outroClienteId = Guid.NewGuid(); // Cliente diferente do dono do veículo
            var ator = new AtorBuilder().ComoCliente(clienteId).Build();
            var ordemServico = new OrdemServicoBuilder().ComOrcamento().Build();

            var veiculo = new VeiculoExternalDtoBuilder().ComClienteId(outroClienteId).Build();

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServico.Id).Retorna(ordemServico);
            _fixture.VeiculoExternalServiceMock.AoObterPorId(ordemServico.VeiculoId).Retorna(veiculo);

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                ator,
                ordemServico.Id,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoErro(
                "Acesso negado. Apenas administradores ou donos da ordem de serviço podem aprovar orçamentos.", 
                ErrorType.NotAllowed);
            _fixture.OperacaoOrdemServicoPresenterMock.NaoDeveTerApresentadoSucesso();
        }

        [Fact(DisplayName = "Deve apresentar erro interno quando ocorrer exceção genérica")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_QuandoOcorreExcecaoGenerica_ApresentaErroInterno()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder().ComItens().ComServicos().ComOrcamento().Build();

            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServico.Id).Retorna(ordemServico);
            _fixture.VeiculoExternalServiceMock.AoObterPorId(ordemServico.VeiculoId).Retorna(new VeiculoExternalDtoBuilder().Build());
            _fixture.OrdemServicoGatewayMock.AoAtualizar().LancaExcecao(new InvalidOperationException("Erro de banco de dados"));

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                new AtorBuilder().ComoAdministrador().Build(),
                ordemServico.Id,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object, 
                MockLogger.CriarSimples());

            // Assert
            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoErro("Erro interno do servidor.", ErrorType.UnexpectedError);
            _fixture.OperacaoOrdemServicoPresenterMock.NaoDeveTerApresentadoSucesso();
        }

        [Fact(DisplayName = "Deve logar information ao ocorrer DomainException")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_AoOcorrerDomainException_LogaInformation()
        {
            // Arrange
            var ator = new AtorBuilder().ComoCliente(Guid.NewGuid()).Build();
            var ordemServicoId = Guid.NewGuid();
            var mockLogger = MockLogger.Criar();

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                ator,
                ordemServicoId,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object,
                mockLogger.Object);

            // Assert
            mockLogger.DeveTerLogadoInformation();
        }

        [Fact(DisplayName = "Deve logar error ao ocorrer Exception")]
        [Trait("UseCase", "AprovarOrcamento")]
        public async Task ExecutarAsync_AoOcorrerException_LogaError()
        {
            // Arrange
            var ator = new AtorBuilder().ComoAdministrador().Build();
            var ordemServicoId = Guid.NewGuid();
            var mockLogger = MockLogger.Criar();
            
            _fixture.OrdemServicoGatewayMock.AoObterPorId(ordemServicoId).LancaExcecao(new InvalidOperationException("Erro de banco de dados"));

            // Act
            await _fixture.AprovarOrcamentoUseCase.ExecutarAsync(
                ator,
                ordemServicoId,
                _fixture.OrdemServicoGatewayMock.Object,
                _fixture.VeiculoExternalServiceMock.Object,
                _fixture.EstoqueMessagePublisherMock.Object,
                _fixture.CorrelationIdAccessorMock.Object,
                _fixture.OperacaoOrdemServicoPresenterMock.Object,
                mockLogger.Object);

            // Assert
            mockLogger.DeveTerLogadoErrorComException();
            _fixture.OperacaoOrdemServicoPresenterMock.DeveTerApresentadoErro("Erro interno do servidor.", ErrorType.UnexpectedError);
        }
    }
}
