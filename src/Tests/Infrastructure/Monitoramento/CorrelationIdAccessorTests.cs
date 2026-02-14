using FluentAssertions;
using Infrastructure.Monitoramento;
using Infrastructure.Monitoramento.Correlation;

namespace Tests.Infrastructure.Monitoramento;

[Trait("Infrastructure", "Monitoramento")]
public class CorrelationIdAccessorTests
{
    [Fact(DisplayName = "GetCorrelationId deve retornar valor atual quando CorrelationContext estiver definido")]
    public void GetCorrelationId_DeveRetornarValorAtual_QuandoCorrelationContextEstiverDefinido()
    {
        // Arrange
        var correlationIdEsperado = "correlation-os-123";
        var accessor = new CorrelationIdAccessor();

        // Act
        string resultado;
        using (CorrelationContext.Push(correlationIdEsperado))
            resultado = accessor.GetCorrelationId();

        // Assert
        resultado.Should().Be(correlationIdEsperado);
    }

    [Fact(DisplayName = "GetCorrelationId deve gerar GUID válido quando CorrelationContext estiver nulo ou whitespace")]
    public void GetCorrelationId_DeveGerarGuidValido_QuandoCorrelationContextEstiverNuloOuWhitespace()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();

        // Act
        var resultado = accessor.GetCorrelationId();

        // Assert
        resultado.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(resultado, out _).Should().BeTrue();
    }

    [Fact(DisplayName = "GetCorrelationId deve usar fallback para GUID quando contexto for whitespace")]
    public void GetCorrelationId_DeveUsarFallbackParaGuid_QuandoContextoForWhitespace()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();

        // Act
        string resultado;
        using (CorrelationContext.Push("   "))
            resultado = accessor.GetCorrelationId();

        // Assert
        resultado.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(resultado, out _).Should().BeTrue();
    }

    [Fact(DisplayName = "GetCorrelationId deve gerar GUIDs diferentes a cada chamada quando não há contexto")]
    public void GetCorrelationId_DeveGerarGuidsDiferentes_QuandoNaoHaContexto()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();

        // Act
        var primeiroResultado = accessor.GetCorrelationId();
        var segundoResultado = accessor.GetCorrelationId();

        // Assert
        primeiroResultado.Should().NotBe(segundoResultado);
        Guid.TryParse(primeiroResultado, out _).Should().BeTrue();
        Guid.TryParse(segundoResultado, out _).Should().BeTrue();
    }

    [Fact(DisplayName = "GetCorrelationId deve respeitar escopos aninhados")]
    public void GetCorrelationId_DeveRespeitarEscoposAninhados()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();
        var correlationIdExterno = "correlation-externo";
        var correlationIdInterno = "correlation-interno";

        // Act & Assert
        using (CorrelationContext.Push(correlationIdExterno))
        {
            accessor.GetCorrelationId().Should().Be(correlationIdExterno);

            using (CorrelationContext.Push(correlationIdInterno))
                accessor.GetCorrelationId().Should().Be(correlationIdInterno);

            accessor.GetCorrelationId().Should().Be(correlationIdExterno);
        }
    }
}
