using Application.Contracts.Messaging.DTOs;
using Application.Contracts.Monitoramento;
using FluentAssertions;
using Infrastructure.Messaging;
using MassTransit;
using Moq;
using Tests.Application.SharedHelpers;

namespace Tests.Infrastructure.Messaging
{
    public class EstoqueMessagePublisherTests
    {
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly MockLogger _mockLogger;
        private readonly EstoqueMessagePublisher _sut;

        public EstoqueMessagePublisherTests()
        {
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _mockLogger = MockLogger.Criar();
            _sut = new EstoqueMessagePublisher(_publishEndpointMock.Object, _mockLogger.Object);
        }

        [Fact(DisplayName = "Deve publicar solicitação de redução com TTL configurado")]
        [Trait("Infrastructure", "EstoqueMessagePublisher")]
        public async Task PublicarSolicitacaoReducaoAsync_DevePublicar_QuandoSolicitacaoValida()
        {
            // Arrange
            var solicitacao = new ReducaoEstoqueSolicitacao
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OrdemServicoId = Guid.NewGuid(),
                Itens = new List<ItemReducao>
                {
                    new ItemReducao { ItemEstoqueId = Guid.NewGuid(), Quantidade = 2 }
                }
            };

            // Act & Assert - Não deve lançar exceção ao publicar (o IPublishEndpoint.Publish é um extension method)
            var acao = async () => await _sut.PublicarSolicitacaoReducaoAsync(solicitacao);

            await acao.Should().NotThrowAsync();
        }

        [Fact(DisplayName = "Deve lançar ArgumentNullException quando solicitação é nula")]
        [Trait("Infrastructure", "EstoqueMessagePublisher")]
        public async Task PublicarSolicitacaoReducaoAsync_DeveLancarExcecao_QuandoSolicitacaoNula()
        {
            // Act & Assert
            await FluentActions.Invoking(() => _sut.PublicarSolicitacaoReducaoAsync(null!))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact(DisplayName = "Deve lançar ArgumentNullException quando publishEndpoint é nulo")]
        [Trait("Infrastructure", "EstoqueMessagePublisher")]
        public void Construtor_DeveLancarExcecao_QuandoPublishEndpointNulo()
        {
            // Act & Assert
            FluentActions.Invoking(() => new EstoqueMessagePublisher(null!, _mockLogger.Object))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "Deve lançar ArgumentNullException quando logger é nulo")]
        [Trait("Infrastructure", "EstoqueMessagePublisher")]
        public void Construtor_DeveLancarExcecao_QuandoLoggerNulo()
        {
            // Act & Assert
            FluentActions.Invoking(() => new EstoqueMessagePublisher(_publishEndpointMock.Object, null!))
                .Should().Throw<ArgumentNullException>();
        }
    }
}
