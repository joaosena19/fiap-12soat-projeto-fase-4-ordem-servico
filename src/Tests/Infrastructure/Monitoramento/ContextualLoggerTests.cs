using Application.Contracts.Monitoramento;
using FluentAssertions;
using Infrastructure.Monitoramento;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Infrastructure.SharedHelpers;
using Xunit;

namespace Tests.Infrastructure.Monitoramento;

public class ContextualLoggerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Dictionary<string, object?> _contextoInicial;
    private readonly ContextualLogger _sut;

    public ContextualLoggerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _contextoInicial = new Dictionary<string, object?>
        {
            ["UserId"] = 123,
            ["SessionId"] = "abc-123"
        };
        _sut = new ContextualLogger(_loggerMock.Object, _contextoInicial);
    }

    [Fact(DisplayName = "Deve logar mensagem de debug com contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogDebug_DeveRepassarChamadaParaLoggerInterno()
    {
        // Arrange
        var messageTemplate = "Debug: processando item {ItemId}";
        var args = new object[] { 999 };

        // Act
        _sut.LogDebug(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoDebug();
    }

    [Fact(DisplayName = "Deve logar mensagem de informação com contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogInformation_DeveRepassarChamadaParaLoggerInterno_QuandoChamado()
    {
        // Arrange
        var messageTemplate = "Operação {Operation} executada com sucesso";
        var args = new object[] { "CriarOrdem" };

        // Act
        _sut.LogInformation(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve logar mensagem de aviso com contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogWarning_DeveRepassarChamadaParaLoggerInterno()
    {
        // Arrange
        var messageTemplate = "Aviso: recurso {Resource} em uso elevado";
        var args = new object[] { "CPU" };

        // Act
        _sut.LogWarning(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoWarning();
    }

    [Fact(DisplayName = "Deve logar erro com mensagem e contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogError_ComMensagem_DeveRepassarChamadaParaLoggerInterno()
    {
        // Arrange
        var messageTemplate = "Erro ao processar pedido {PedidoId}";
        var args = new object[] { 789 };

        // Act
        _sut.LogError(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoError();
    }

    [Fact(DisplayName = "Deve logar erro com exceção e contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogError_DeveRepassarException_QuandoChamado()
    {
        // Arrange
        var exception = new InvalidOperationException("Operação inválida");
        var messageTemplate = "Falha ao executar {Operation}";
        var args = new object[] { "AtualizarEstoque" };

        // Act
        _sut.LogError(exception, messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoError();
    }

    [Fact(DisplayName = "Deve logar crítico com mensagem e contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogCritical_ComMensagem_DeveRepassarChamadaParaLoggerInterno()
    {
        // Arrange
        var messageTemplate = "Falha crítica no componente {Component}";
        var args = new object[] { "Database" };

        // Act
        _sut.LogCritical(messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoCritical();
    }

    [Fact(DisplayName = "Deve logar crítico com exceção e contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogCritical_DeveRepassarException_QuandoChamado()
    {
        // Arrange
        var exception = new Exception("Erro crítico");
        var messageTemplate = "Sistema em falha crítica: {Detalhes}";
        var args = new object[] { "Perda de conexão" };

        // Act
        _sut.LogCritical(exception, messageTemplate, args);

        // Assert
        _loggerMock.DeveTerLogadoCritical();
    }

    [Fact(DisplayName = "Deve retornar novo logger com contexto adicional")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void ComPropriedade_DeveRetornarLoggerComContexto_QuandoChamado()
    {
        // Arrange
        var key = "CorrelationId";
        var value = Guid.NewGuid();

        // Act
        var resultado = _sut.ComPropriedade(key, value);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeOfType<ContextualLogger>();
        resultado.Should().NotBeSameAs(_sut);
    }

    [Fact(DisplayName = "Logger com contexto adicional deve manter contexto original")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void ComPropriedade_DeveManterContextoOriginal_QuandoCriarNovoLogger()
    {
        // Arrange
        var novaChave = "TraceId";
        var novoValor = "trace-xyz";
        var messageTemplate = "Mensagem com contexto completo";

        // Act
        var novoLogger = _sut.ComPropriedade(novaChave, novoValor);
        novoLogger.LogInformation(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve encadear múltiplas propriedades de contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void ComPropriedade_DevePermitirEncadeamento_QuandoChamadoMultiplasVezes()
    {
        // Arrange
        var messageTemplate = "Operação complexa executada";

        // Act
        var loggerComContexto = _sut.ComPropriedade("Prop1", "Valor1").ComPropriedade("Prop2", 42).ComPropriedade("Prop3", true);
        loggerComContexto.LogInformation(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve permitir valor nulo na propriedade de contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void ComPropriedade_DevePermitirValorNulo_QuandoChamado()
    {
        // Arrange
        var key = "OptionalProperty";
        object? value = null;

        // Act
        var resultado = _sut.ComPropriedade(key, value);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeOfType<ContextualLogger>();
    }

    [Fact(DisplayName = "Deve logar com contexto vazio quando criado sem contexto")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void ContextualLogger_DeveLogarCorretamente_QuandoCriadoComContextoVazio()
    {
        // Arrange
        var contextoVazio = new Dictionary<string, object?>();
        var loggerSemContexto = new ContextualLogger(_loggerMock.Object, contextoVazio);
        var messageTemplate = "Mensagem sem contexto adicional";

        // Act
        loggerSemContexto.LogInformation(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve logar todas as mensagens sem argumentos")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogInformation_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Operação simples concluída";

        // Act
        _sut.LogInformation(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoInformation();
    }

    [Fact(DisplayName = "Deve logar debug sem argumentos")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogDebug_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Debug sem argumentos";

        // Act
        _sut.LogDebug(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoDebug();
    }

    [Fact(DisplayName = "Deve logar warning sem argumentos")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogWarning_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Aviso sem argumentos";

        // Act
        _sut.LogWarning(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoWarning();
    }

    [Fact(DisplayName = "Deve logar erro sem argumentos")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogError_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Erro sem argumentos";

        // Act
        _sut.LogError(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoError();
    }

    [Fact(DisplayName = "Deve logar crítico sem argumentos")]
    [Trait("Infrastructure", "ContextualLogger")]
    public void LogCritical_ComParametrosVazios_DeveLogarCorretamente()
    {
        // Arrange
        var messageTemplate = "Crítico sem argumentos";

        // Act
        _sut.LogCritical(messageTemplate);

        // Assert
        _loggerMock.DeveTerLogadoCritical();
    }
}
