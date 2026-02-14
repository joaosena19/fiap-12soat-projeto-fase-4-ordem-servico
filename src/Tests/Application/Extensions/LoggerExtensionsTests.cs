using Application.Extensions;
using Application.Extensions.Enums;
using FluentAssertions;
using Moq;
using Tests.Application.SharedHelpers;
using Xunit;

namespace Tests.Application.Extensions;

public class LoggerExtensionsTests
{
    #region ComMensageria

    [Fact(DisplayName = "Deve adicionar propriedades de mensageria quando chamado")]
    [Trait("Application", "LoggerExtensions")]
    public void ComMensageria_DeveAdicionarPropriedadesDeMensageria_QuandoChamado()
    {
        // Arrange
        var mockLogger = MockLogger.Criar();
        var nomeMensagem = NomeMensagemEnum.ReducaoEstoqueSolicitacao;
        var tipoMensagem = TipoMensagemEnum.Publicacao;

        // Act
        var resultado = mockLogger.Object.ComMensageria(nomeMensagem, tipoMensagem);

        // Assert
        resultado.Should().NotBeNull();
        mockLogger.DeveTerChamadoComPropriedade("Mensagem_Nome", nomeMensagem.ToString());
        mockLogger.DeveTerChamadoComPropriedade("Mensagem_Tipo", tipoMensagem.ToString());
        mockLogger.DeveTerChamadoComPropriedade("Eh_Mensageria", true);
    }

    [Theory(DisplayName = "Deve adicionar propriedades com diferentes tipos de mensagem")]
    [InlineData(NomeMensagemEnum.ReducaoEstoqueSolicitacao, TipoMensagemEnum.Publicacao)]
    [InlineData(NomeMensagemEnum.ReducaoEstoqueResultado, TipoMensagemEnum.Consumo)]
    [Trait("Application", "LoggerExtensions")]
    public void ComMensageria_DeveAdicionarPropriedades_ParaDiferentesMensagens(NomeMensagemEnum nomeMensagem, TipoMensagemEnum tipoMensagem)
    {
        // Arrange
        var mockLogger = MockLogger.Criar();

        // Act
        var resultado = mockLogger.Object.ComMensageria(nomeMensagem, tipoMensagem);

        // Assert
        resultado.Should().NotBeNull();
        mockLogger.DeveTerChamadoComPropriedade("Mensagem_Nome", nomeMensagem.ToString());
        mockLogger.DeveTerChamadoComPropriedade("Mensagem_Tipo", tipoMensagem.ToString());
        mockLogger.DeveTerChamadoComPropriedade("Eh_Mensageria", true);
    }

    #endregion

    #region ComAtor

    [Fact(DisplayName = "Deve adicionar propriedades do ator quando chamado")]
    [Trait("Application", "LoggerExtensions")]
    public void ComAtor_DeveAdicionarPropriedadesDoAtor_QuandoChamado()
    {
        // Arrange
        var mockLogger = MockLogger.Criar();
        var clienteId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();

        // Act
        var resultado = mockLogger.Object.ComAtor(ator);

        // Assert
        resultado.Should().NotBeNull();
        mockLogger.DeveTerChamadoComPropriedade("Ator_UsuarioId", ator.UsuarioId);
        mockLogger.DeveTerChamadoComPropriedade("Ator_ClienteId", (object?)ator.ClienteId!);
        mockLogger.Mock.Verify(x => x.ComPropriedade("Ator_UsuarioRoles", It.IsAny<string[]>()), Times.Once);
    }

    [Fact(DisplayName = "Deve adicionar propriedades do ator administrador quando chamado")]
    [Trait("Application", "LoggerExtensions")]
    public void ComAtor_DeveAdicionarPropriedadesDoAdministrador_QuandoChamado()
    {
        // Arrange
        var mockLogger = MockLogger.Criar();
        var ator = new AtorBuilder().ComoAdministrador().Build();

        // Act
        var resultado = mockLogger.Object.ComAtor(ator);

        // Assert
        resultado.Should().NotBeNull();
        mockLogger.DeveTerChamadoComPropriedade("Ator_UsuarioId", ator.UsuarioId);
        mockLogger.Mock.Verify(x => x.ComPropriedade("Ator_ClienteId", null!), Times.Once);
        mockLogger.Mock.Verify(x => x.ComPropriedade("Ator_UsuarioRoles", It.IsAny<string[]>()), Times.Once);
    }

    #endregion
}
