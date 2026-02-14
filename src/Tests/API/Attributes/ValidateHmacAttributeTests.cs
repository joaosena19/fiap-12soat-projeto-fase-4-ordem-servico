using API.Attributes;
using FluentAssertions;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;

namespace Tests.API.Attributes
{
    public class ValidateHmacAttributeTests
    {
        private readonly ValidateHmacAttribute _sut;

        public ValidateHmacAttributeTests()
        {
            _sut = new ValidateHmacAttribute();
        }

        private (ResourceExecutingContext context, Mock<ResourceExecutionDelegate> next) CriarContexto(string? signatureHeader, string? body, Mock<IHmacValidationService>? hmacServiceMock = null)
        {
            var httpContext = new DefaultHttpContext();

            if (signatureHeader != null)
                httpContext.Request.Headers["X-Signature"] = signatureHeader;

            if (body != null)
            {
                var bodyBytes = Encoding.UTF8.GetBytes(body);
                httpContext.Request.Body = new MemoryStream(bodyBytes);
                httpContext.Request.ContentLength = bodyBytes.Length;
            }
            else
            {
                httpContext.Request.Body = new MemoryStream();
            }

            var serviceCollection = new ServiceCollection();
            if (hmacServiceMock != null)
                serviceCollection.AddSingleton(hmacServiceMock.Object);

            httpContext.RequestServices = serviceCollection.BuildServiceProvider();

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var filters = new List<IFilterMetadata>();

            var executingContext = new ResourceExecutingContext(actionContext, filters, new List<IValueProviderFactory>());
            var nextMock = new Mock<ResourceExecutionDelegate>();
            nextMock.Setup(n => n()).ReturnsAsync(new ResourceExecutedContext(actionContext, filters));

            return (executingContext, nextMock);
        }

        [Fact(DisplayName = "Deve retornar 401 quando header X-Signature ausente")]
        [Trait("Attribute", "ValidateHmacAttribute")]
        public async Task OnResourceExecutionAsync_DeveRetornar401_QuandoSignatureAusente()
        {
            // Arrange
            var (context, next) = CriarContexto(null, "{\"data\": \"test\"}");

            // Act
            await _sut.OnResourceExecutionAsync(context, next.Object);

            // Assert
            context.Result.Should().BeOfType<UnauthorizedObjectResult>();
            next.Verify(n => n(), Times.Never);
        }

        [Fact(DisplayName = "Deve retornar 401 quando header X-Signature vazio")]
        [Trait("Attribute", "ValidateHmacAttribute")]
        public async Task OnResourceExecutionAsync_DeveRetornar401_QuandoSignatureVazia()
        {
            // Arrange
            var (context, next) = CriarContexto("", "{\"data\": \"test\"}");

            // Act
            await _sut.OnResourceExecutionAsync(context, next.Object);

            // Assert
            context.Result.Should().BeOfType<UnauthorizedObjectResult>();
            next.Verify(n => n(), Times.Never);
        }

        [Fact(DisplayName = "Deve retornar 401 quando assinatura inválida")]
        [Trait("Attribute", "ValidateHmacAttribute")]
        public async Task OnResourceExecutionAsync_DeveRetornar401_QuandoAssinaturaInvalida()
        {
            // Arrange
            var hmacServiceMock = new Mock<IHmacValidationService>();
            hmacServiceMock.Setup(h => h.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var (context, next) = CriarContexto("sha256=assinatura-invalida", "{\"data\": \"test\"}", hmacServiceMock);

            // Act
            await _sut.OnResourceExecutionAsync(context, next.Object);

            // Assert
            context.Result.Should().BeOfType<UnauthorizedObjectResult>();
            next.Verify(n => n(), Times.Never);
        }

        [Fact(DisplayName = "Deve chamar next quando assinatura válida")]
        [Trait("Attribute", "ValidateHmacAttribute")]
        public async Task OnResourceExecutionAsync_DeveChamarNext_QuandoAssinaturaValida()
        {
            // Arrange
            var hmacServiceMock = new Mock<IHmacValidationService>();
            hmacServiceMock.Setup(h => h.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var (context, next) = CriarContexto("sha256=assinatura-valida", "{\"data\": \"test\"}", hmacServiceMock);

            // Act
            await _sut.OnResourceExecutionAsync(context, next.Object);

            // Assert
            context.Result.Should().BeNull();
            next.Verify(n => n(), Times.Once);
        }

        [Fact(DisplayName = "Deve resetar posição do body após leitura")]
        [Trait("Attribute", "ValidateHmacAttribute")]
        public async Task OnResourceExecutionAsync_DeveResetarPosicaoBody_AposLeitura()
        {
            // Arrange
            var hmacServiceMock = new Mock<IHmacValidationService>();
            hmacServiceMock.Setup(h => h.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var bodyContent = "{\"data\": \"test\"}";
            var (context, next) = CriarContexto("sha256=assinatura", bodyContent, hmacServiceMock);

            // Act
            await _sut.OnResourceExecutionAsync(context, next.Object);

            // Assert
            context.HttpContext.Request.Body.Position.Should().Be(0);
        }

        [Fact(DisplayName = "Deve validar payload correto com o serviço HMAC")]
        [Trait("Attribute", "ValidateHmacAttribute")]
        public async Task OnResourceExecutionAsync_DeveValidarPayloadCorreto_ComServicoHmac()
        {
            // Arrange
            var payload = "{\"ordemServicoId\": \"123\"}";
            var hmacServiceMock = new Mock<IHmacValidationService>();
            hmacServiceMock.Setup(h => h.ValidateSignature(payload, "assinatura-teste")).Returns(true);

            var (context, next) = CriarContexto("assinatura-teste", payload, hmacServiceMock);

            // Act
            await _sut.OnResourceExecutionAsync(context, next.Object);

            // Assert
            hmacServiceMock.Verify(h => h.ValidateSignature(payload, "assinatura-teste"), Times.Once);
        }
    }
}
