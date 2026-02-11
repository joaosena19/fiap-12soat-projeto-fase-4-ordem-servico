using Application.Contracts.Messaging;
using Infrastructure.Messaging.DTOs;
using Infrastructure.Messaging;
using MassTransit;
using Moq;
using Xunit;

namespace Tests.Infrastructure.Messaging;

/// <summary>
/// Testes unitários para EstoqueMessagePublisher.
/// Valida a configuração de TTL, publicação de mensagens e tratamento de erros.
/// </summary>
public class EstoqueMessagePublisherTests
{
    [Fact]
    public async Task PublicarSolicitacaoReducaoAsync_WhenCalled_PublicaComTtl60Segundos()
    {
        // Arrange
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EstoqueMessagePublisher(publishEndpointMock.Object);

        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Itens = new List<ItemReducao>
            {
                new ItemReducao { ItemEstoqueId = Guid.NewGuid(), Quantidade = 5 }
            }
        };

        TimeSpan? capturedTtl = null;
        publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<ReducaoEstoqueSolicitacao>(),
                It.IsAny<Action<PublishContext<ReducaoEstoqueSolicitacao>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, Action<PublishContext>, CancellationToken>((msg, callback, ct) =>
            {
                // Criar um mock de PublishContext para capturar o TTL
                var contextMock = new Mock<PublishContext<ReducaoEstoqueSolicitacao>>();
                contextMock.SetupProperty(x => x.TimeToLive);
                callback(contextMock.Object);
                capturedTtl = contextMock.Object.TimeToLive;
            })
            .Returns(Task.CompletedTask);

        // Act
        await publisher.PublicarSolicitacaoReducaoAsync(solicitacao);

        // Assert
        publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<ReducaoEstoqueSolicitacao>(s => s.CorrelationId == solicitacao.CorrelationId),
                It.IsAny<Action<PublishContext<ReducaoEstoqueSolicitacao>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(capturedTtl);
        Assert.Equal(TimeSpan.FromSeconds(60), capturedTtl.Value);
    }

    [Fact]
    public async Task PublicarSolicitacaoReducaoAsync_WhenCalled_PublicaMensagemComDadosCorretos()
    {
        // Arrange
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EstoqueMessagePublisher(publishEndpointMock.Object);

        var correlationId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();

        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = correlationId,
            OrdemServicoId = ordemServicoId,
            Itens = new List<ItemReducao>
            {
                new ItemReducao { ItemEstoqueId = itemId1, Quantidade = 3 },
                new ItemReducao { ItemEstoqueId = itemId2, Quantidade = 7 }
            }
        };

        ReducaoEstoqueSolicitacao? capturedMessage = null;
        publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<ReducaoEstoqueSolicitacao>(),
                It.IsAny<Action<PublishContext<ReducaoEstoqueSolicitacao>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, Action<PublishContext>, CancellationToken>((msg, _, _) =>
            {
                capturedMessage = msg as ReducaoEstoqueSolicitacao;
            })
            .Returns(Task.CompletedTask);

        // Act
        await publisher.PublicarSolicitacaoReducaoAsync(solicitacao);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Equal(correlationId, capturedMessage.CorrelationId);
        Assert.Equal(ordemServicoId, capturedMessage.OrdemServicoId);
        Assert.Equal(2, capturedMessage.Itens.Count);
        Assert.Equal(itemId1, capturedMessage.Itens[0].ItemEstoqueId);
        Assert.Equal(3, capturedMessage.Itens[0].Quantidade);
        Assert.Equal(itemId2, capturedMessage.Itens[1].ItemEstoqueId);
        Assert.Equal(7, capturedMessage.Itens[1].Quantidade);
    }

    [Fact]
    public async Task PublicarSolicitacaoReducaoAsync_WhenSolicitacaoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EstoqueMessagePublisher(publishEndpointMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await publisher.PublicarSolicitacaoReducaoAsync(null!));

        publishEndpointMock.Verify(
            x => x.Publish(
                It.IsAny<ReducaoEstoqueSolicitacao>(),
                It.IsAny<Action<PublishContext<ReducaoEstoqueSolicitacao>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_WhenPublishEndpointIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EstoqueMessagePublisher(null!));
    }

    [Fact]
    public async Task PublicarSolicitacaoReducaoAsync_WhenPublishEndpointThrows_PropagatesException()
    {
        // Arrange
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var expectedException = new InvalidOperationException("SQS connection failed");
        
        publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<ReducaoEstoqueSolicitacao>(),
                It.IsAny<Action<PublishContext<ReducaoEstoqueSolicitacao>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var publisher = new EstoqueMessagePublisher(publishEndpointMock.Object);

        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Itens = new List<ItemReducao>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await publisher.PublicarSolicitacaoReducaoAsync(solicitacao));
        
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task PublicarSolicitacaoReducaoAsync_WhenItensIsEmpty_PublicaMensagemNormalmente()
    {
        // Arrange
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EstoqueMessagePublisher(publishEndpointMock.Object);

        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Itens = new List<ItemReducao>() // Lista vazia
        };

        // Act
        await publisher.PublicarSolicitacaoReducaoAsync(solicitacao);

        // Assert
        publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<ReducaoEstoqueSolicitacao>(s => 
                    s.CorrelationId == solicitacao.CorrelationId &&
                    s.Itens.Count == 0),
                It.IsAny<Action<PublishContext<ReducaoEstoqueSolicitacao>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

}
