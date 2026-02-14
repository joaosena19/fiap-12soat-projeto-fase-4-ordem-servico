using System.Net;
using System.Text.Json;
using API.Dtos;
using API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Enums;
using Shared.Exceptions;

namespace Tests.API.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
        private readonly DefaultHttpContext _httpContext;

        public ExceptionHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        [Fact(DisplayName = "InvokeAsync deve chamar o próximo delegate quando não ocorrer exceção")]
        [Trait("Middleware", "ExceptionHandlingMiddleware")]
        public async Task InvokeAsync_DeveChamarProximoDelegate_Quando_SemExcecao()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            nextCalled.Should().BeTrue();
            // O status code padrão do DefaultHttpContext é 200, mas vamos garantir que não foi alterado para erro
            ((int)_httpContext.Response.StatusCode).Should().BeInRange(200, 299);
        }

        [Theory(DisplayName = "InvokeAsync com DomainException deve mapear para StatusCode correto")]
        [Trait("Middleware", "ExceptionHandlingMiddleware")]
        [InlineData(ErrorType.InvalidInput, HttpStatusCode.BadRequest)]
        [InlineData(ErrorType.ResourceNotFound, HttpStatusCode.NotFound)]
        [InlineData(ErrorType.ReferenceNotFound, HttpStatusCode.UnprocessableEntity)]
        [InlineData(ErrorType.DomainRuleBroken, HttpStatusCode.UnprocessableEntity)]
        [InlineData(ErrorType.Conflict, HttpStatusCode.Conflict)]
        [InlineData(ErrorType.Unauthorized, HttpStatusCode.Unauthorized)]
        [InlineData(ErrorType.NotAllowed, HttpStatusCode.Forbidden)]
        [InlineData(ErrorType.BadGateway, HttpStatusCode.BadGateway)]
        [InlineData(ErrorType.UnexpectedError, HttpStatusCode.InternalServerError)]
        public async Task InvokeAsync_DeveMapearStatusCode_Quando_DomainException(ErrorType errorType, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var errorMessage = "Erro de domínio simulado";
            var domainException = new DomainException(errorMessage, errorType);

            RequestDelegate next = (ctx) => throw domainException;

            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be((int)expectedStatusCode);
            _httpContext.Response.ContentType.Should().Be("application/json");

            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseBody, JsonOptions);

            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be(errorMessage);
            errorResponse.StatusCode.Should().Be((int)expectedStatusCode);

            // Verify logging - LogWarning for DomainException
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(errorMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "InvokeAsync com Exception genérica deve retornar 500")]
        [Trait("Middleware", "ExceptionHandlingMiddleware")]
        public async Task InvokeAsync_DeveRetornar500_Quando_ExceptionGenerica()
        {
            // Arrange
            var errorMessage = "Erro inesperado";
            var exception = new Exception(errorMessage);

            RequestDelegate next = (ctx) => throw exception;

            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            _httpContext.Response.ContentType.Should().Be("application/json");

            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseBody, JsonOptions);

            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be("Ocorreu um erro interno no servidor."); // Mensagem fixa definida no middleware
            errorResponse.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            // Verify logging - LogError for generic Exception
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(errorMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
