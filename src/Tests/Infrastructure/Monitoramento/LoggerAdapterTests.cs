using FluentAssertions;
using Infrastructure.Monitoramento;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Infrastructure.SharedHelpers;
using Xunit;

namespace Tests.Infrastructure.Monitoramento;

public class LoggerAdapterTests
{
    private readonly Mock<ILogger<LoggerAdapterTests>> _loggerMock;
    private readonly LoggerAdapter<LoggerAdapterTests> _sut;

    public LoggerAdapterTests()
    {
        _loggerMock = new Mock<ILogger<LoggerAdapterTests>>();
        _sut = new LoggerAdapter<LoggerAdapterTests>(_loggerMock.Object);
    }

    [Fact(DisplayName = "Deve logar mensagem de debug com template e argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogDebug_DeveLogarMensagemComTemplateEArgumentos()
    {
        // Arrange
        var messageTemplate = "Debug: processando item {ItemId}";
        var args = new object[] { 999 };

        // Act
        _sut.LogDebug(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoDebug();
    }

    [Fact(DisplayName = "Deve logar mensagem de informação com template e argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogInformation_DeveLogarMensagemComTemplateEArgumentos()
    {
        // Arrange
        var messageTemplate = "Usuário {UserId} realizou operação {Operation}";
        var args = new object[] { 123, "Login" };

        // Act
        _sut.LogInformation(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve logar mensagem de aviso com template e argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogWarning_DeveLogarMensagemComTemplateEArgumentos()
    {
        // Arrange
        var messageTemplate = "Atenção: recurso {Resource} está com uso elevado";
        var args = new object[] { "Memória" };

        // Act
        _sut.LogWarning(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoWarning();
    }

    [Fact(DisplayName = "Deve logar erro com template e argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogError_ComMensagem_DeveLogarErroComTemplateEArgumentos()
    {
        // Arrange
        var messageTemplate = "Erro ao processar pedido {OrderId}";
        var args = new object[] { 456 };

        // Act
        _sut.LogError(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoError();
    }

    [Fact(DisplayName = "Deve logar erro com exceção, template e argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogError_ComException_DeveLogarErroComExceptionTemplateEArgumentos()
    {
        // Arrange
        var exception = new InvalidOperationException("Operação inválida");
        var messageTemplate = "Falha ao executar operação {Operation}";
        var args = new object[] { "ProcessarPagamento" };

        // Act
        _sut.LogError(exception, messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoError();
    }

    [Fact(DisplayName = "Deve logar crítico com template e argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogCritical_ComMensagem_DeveLogarCriticoComTemplateEArgumentos()
    {
        // Arrange
        var messageTemplate = "Falha crítica no componente {Component}";
        var args = new object[] { "Database" };

        // Act
        _sut.LogCritical(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoCritical();
    }

    [Fact(DisplayName = "Deve logar crítico com exceção, template e argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogCritical_ComException_DeveLogarCriticoComExceptionTemplateEArgumentos()
    {
        // Arrange
        var exception = new Exception("Erro crítico");
        var messageTemplate = "Sistema em falha crítica: {Detalhes}";
        var args = new object[] { "Perda de conexão com banco" };

        // Act
        _sut.LogCritical(exception, messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoCritical();
    }

    [Fact(DisplayName = "Deve retornar ContextualLogger com propriedade inicial")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void ComPropriedade_DeveRetornarContextualLoggerComPropriedadeInicial()
    {
        // Arrange
        var key = "CorrelationId";
        var value = Guid.NewGuid();

        // Act
        var result = _sut.ComPropriedade(key, value);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContextualLogger>();
    }

    [Fact(DisplayName = "ContextualLogger retornado por ComPropriedade deve ser capaz de logar")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void ComPropriedade_ContextualLoggerRetornado_DeveSerCapazDeLogar()
    {
        // Arrange
        var key = "RequestId";
        var value = "req-123";
        var messageTemplate = "Processando requisição";

        // Act
        var contextualLogger = _sut.ComPropriedade(key, value);
        contextualLogger.LogInformation(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve logar informação sem argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogInformation_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Operação concluída com sucesso";

        // Act
        _sut.LogInformation(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve logar aviso sem argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogWarning_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Aviso geral do sistema";

        // Act
        _sut.LogWarning(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoWarning();
    }

    [Fact(DisplayName = "Deve logar erro sem argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogError_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Erro crítico no sistema";

        // Act
        _sut.LogError(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoError();
    }

    [Fact(DisplayName = "Deve logar debug sem argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogDebug_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Debug: operação iniciada";

        // Act
        _sut.LogDebug(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoDebug();
    }

    [Fact(DisplayName = "Deve logar crítico sem argumentos")]
    [Trait("Infrastructure", "LoggerAdapter")]
    public void LogCritical_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Falha crítica detectada";

        // Act
        _sut.LogCritical(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoCritical();
    }
}
