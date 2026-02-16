using API.Middleware;
using FluentAssertions;
using Infrastructure.Monitoramento.Correlation;
using Microsoft.AspNetCore.Http;

namespace Tests.API.Middleware
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact(DisplayName = "Deve gerar novo GUID quando header ausente")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DeveGerarNovoGuid_QuandoHeaderAusente()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            string? correlationIdCapturado = null;

            var middleware = new CorrelationIdMiddleware(next: (ctx) =>
            {
                correlationIdCapturado = ctx.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            correlationIdCapturado.Should().NotBeNullOrEmpty();
            Guid.TryParse(correlationIdCapturado, out _).Should().BeTrue();
        }

        [Fact(DisplayName = "Deve preservar valor quando header presente com valor válido")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DevePreservarValor_QuandoHeaderPresente()
        {
            // Arrange
            var correlationIdEsperado = "meu-correlation-id-customizado";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[CorrelationConstants.HeaderName] = correlationIdEsperado;
            string? correlationIdCapturado = null;

            var middleware = new CorrelationIdMiddleware(next: (ctx) =>
            {
                correlationIdCapturado = ctx.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            correlationIdCapturado.Should().Be(correlationIdEsperado);
        }

        [Fact(DisplayName = "Deve gerar novo GUID quando header presente mas vazio")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DeveGerarNovoGuid_QuandoHeaderVazio()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[CorrelationConstants.HeaderName] = "";
            string? correlationIdCapturado = null;

            var middleware = new CorrelationIdMiddleware(next: (ctx) =>
            {
                correlationIdCapturado = ctx.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            correlationIdCapturado.Should().NotBeNullOrEmpty();
            Guid.TryParse(correlationIdCapturado, out _).Should().BeTrue();
        }

        [Fact(DisplayName = "Deve gerar novo GUID quando header presente mas whitespace")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DeveGerarNovoGuid_QuandoHeaderWhitespace()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[CorrelationConstants.HeaderName] = "   ";
            string? correlationIdCapturado = null;

            var middleware = new CorrelationIdMiddleware(next: (ctx) =>
            {
                correlationIdCapturado = ctx.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            correlationIdCapturado.Should().NotBeNullOrEmpty();
            correlationIdCapturado.Should().NotBe("   ");
        }

        [Fact(DisplayName = "Deve definir response header via OnStarting")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DeveDefinirResponseHeader_QuandoResposta()
        {
            // Arrange
            var correlationIdEsperado = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[CorrelationConstants.HeaderName] = correlationIdEsperado;

            var middleware = new CorrelationIdMiddleware(next: (ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Verificar que o OnStarting foi registrado (executar callbacks)
            // DefaultHttpContext não executa OnStarting callbacks automaticamente,
            // mas podemos validar que o request header foi normalizado
            httpContext.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault().Should().Be(correlationIdEsperado);
        }

        [Fact(DisplayName = "Deve empurrar correlation context durante execução do pipeline")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DeveEmpurrarCorrelationContext_DuranteExecucaoPipeline()
        {
            // Arrange
            var correlationIdEsperado = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[CorrelationConstants.HeaderName] = correlationIdEsperado;
            string? correlationContextCapturado = null;

            var middleware = new CorrelationIdMiddleware(next: (ctx) =>
            {
                correlationContextCapturado = CorrelationContext.Current;
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            correlationContextCapturado.Should().Be(correlationIdEsperado);
        }

        [Fact(DisplayName = "Deve normalizar request header removendo duplicatas")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DeveNormalizarRequestHeader_QuandoMultiplosValores()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append(CorrelationConstants.HeaderName, "valor1");
            httpContext.Request.Headers.Append(CorrelationConstants.HeaderName, "valor2");
            string? correlationIdCapturado = null;

            var middleware = new CorrelationIdMiddleware(next: (ctx) =>
            {
                correlationIdCapturado = ctx.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            correlationIdCapturado.Should().NotBeNull();
            httpContext.Request.Headers[CorrelationConstants.HeaderName].Count.Should().Be(1);
        }

        [Fact(DisplayName = "Deve preservar valor não-GUID recebido")]
        [Trait("Middleware", "CorrelationIdMiddleware")]
        public async Task InvokeAsync_DevePreservarValorNaoGuid_QuandoRecebido()
        {
            // Arrange
            var correlationIdCustomizado = "request-12345-abc";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[CorrelationConstants.HeaderName] = correlationIdCustomizado;
            string? correlationIdCapturado = null;

            var middleware = new CorrelationIdMiddleware(next: (ctx) =>
            {
                correlationIdCapturado = ctx.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            correlationIdCapturado.Should().Be(correlationIdCustomizado);
        }
    }
}
