using API.Endpoints;
using Application.Identidade.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.API.Endpoints
{
    /// <summary>
    /// Implementação concreta de BaseController para testes
    /// </summary>
    public class BaseControllerConcreta : BaseController
    {
        public BaseControllerConcreta(ILoggerFactory loggerFactory) : base(loggerFactory) { }

        public Ator ChamarBuscarAtorAtual() => BuscarAtorAtual();
    }

    public class BaseControllerTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly BaseControllerConcreta _sut;

        public BaseControllerTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _sut = new BaseControllerConcreta(_loggerFactoryMock.Object);
        }

        private void ConfigurarHttpContext(string? authorizationHeader = null)
        {
            var httpContext = new DefaultHttpContext();
            if (authorizationHeader != null)
                httpContext.Request.Headers["Authorization"] = authorizationHeader;

            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact(DisplayName = "Deve retornar ator quando token JWT válido")]
        [Trait("Controller", "BaseController")]
        public void BuscarAtorAtual_DeveRetornarAtor_QuandoTokenValido()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var token = new JwtTokenBuilder()
                .ComUsuarioId(usuarioId)
                .ComRole(RoleEnum.Administrador)
                .Build();

            ConfigurarHttpContext($"Bearer {token}");

            // Act
            var ator = _sut.ChamarBuscarAtorAtual();

            // Assert
            ator.Should().NotBeNull();
            ator.UsuarioId.Should().Be(usuarioId);
            ator.Roles.Should().Contain(RoleEnum.Administrador);
        }

        [Fact(DisplayName = "Deve lançar exceção quando header Authorization ausente")]
        [Trait("Controller", "BaseController")]
        public void BuscarAtorAtual_DeveLancarExcecao_QuandoAuthorizationAusente()
        {
            // Arrange
            ConfigurarHttpContext(null);

            // Act & Assert
            FluentActions.Invoking(() => _sut.ChamarBuscarAtorAtual())
                .Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*obrigatório*");
        }

        [Fact(DisplayName = "Deve lançar exceção quando header Authorization vazio")]
        [Trait("Controller", "BaseController")]
        public void BuscarAtorAtual_DeveLancarExcecao_QuandoAuthorizationVazio()
        {
            // Arrange
            ConfigurarHttpContext("");

            // Act & Assert
            FluentActions.Invoking(() => _sut.ChamarBuscarAtorAtual())
                .Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*obrigatório*");
        }

        [Fact(DisplayName = "Deve lançar exceção quando header não inicia com Bearer")]
        [Trait("Controller", "BaseController")]
        public void BuscarAtorAtual_DeveLancarExcecao_QuandoNaoIniciaComBearer()
        {
            // Arrange
            ConfigurarHttpContext("Basic abc123");

            // Act & Assert
            FluentActions.Invoking(() => _sut.ChamarBuscarAtorAtual())
                .Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*obrigatório*");
        }

        [Fact(DisplayName = "Deve retornar ator com clienteId quando token contém clienteId")]
        [Trait("Controller", "BaseController")]
        public void BuscarAtorAtual_DeveRetornarAtorComClienteId_QuandoTokenContemClienteId()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var token = new JwtTokenBuilder()
                .ComUsuarioId(usuarioId)
                .ComClienteId(clienteId)
                .ComRole(RoleEnum.Cliente)
                .Build();

            ConfigurarHttpContext($"Bearer {token}");

            // Act
            var ator = _sut.ChamarBuscarAtorAtual();

            // Assert
            ator.Should().NotBeNull();
            ator.UsuarioId.Should().Be(usuarioId);
            ator.ClienteId.Should().Be(clienteId);
            ator.Roles.Should().Contain(RoleEnum.Cliente);
        }
    }
}
