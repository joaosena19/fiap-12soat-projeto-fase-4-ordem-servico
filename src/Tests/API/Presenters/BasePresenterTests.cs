using API.Presenters;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;

namespace Tests.API.Presenters
{
    /// <summary>
    /// Implementação concreta de BasePresenter para testes
    /// </summary>
    public class BasePresenterConcreta : BasePresenter
    {
        public void ChamarDefinirSucessoComActionResult(IActionResult resultado) => DefinirSucesso(resultado);

        public void ChamarDefinirSucessoComObjeto(object dados) => DefinirSucesso(dados);

        public void ChamarDefinirSucessoComLocalizacao(string action, string controller, object routeValues, object dados) => DefinirSucessoComLocalizacao(action, controller, routeValues, dados);
    }

    public class BasePresenterTests
    {
        #region ApresentarErro

        [Fact(DisplayName = "Deve retornar ConflictObjectResult quando ErrorType é Conflict")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornarConflict_QuandoErrorTypeConflict()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Conflito detectado", ErrorType.Conflict);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            presenter.ObterResultado().Should().BeOfType<ConflictObjectResult>();
        }

        [Fact(DisplayName = "Deve retornar BadRequestObjectResult quando ErrorType é InvalidInput")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornarBadRequest_QuandoErrorTypeInvalidInput()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Entrada inválida", ErrorType.InvalidInput);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            presenter.ObterResultado().Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "Deve retornar NotFoundObjectResult quando ErrorType é ResourceNotFound")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornarNotFound_QuandoErrorTypeResourceNotFound()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Recurso não encontrado", ErrorType.ResourceNotFound);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            presenter.ObterResultado().Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "Deve retornar UnprocessableEntityObjectResult quando ErrorType é ReferenceNotFound")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornarUnprocessableEntity_QuandoErrorTypeReferenceNotFound()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Referência não encontrada", ErrorType.ReferenceNotFound);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            presenter.ObterResultado().Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [Fact(DisplayName = "Deve retornar UnprocessableEntityObjectResult quando ErrorType é DomainRuleBroken")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornarUnprocessableEntity_QuandoErrorTypeDomainRuleBroken()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Regra de domínio violada", ErrorType.DomainRuleBroken);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            presenter.ObterResultado().Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [Fact(DisplayName = "Deve retornar UnauthorizedObjectResult quando ErrorType é Unauthorized")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornarUnauthorized_QuandoErrorTypeUnauthorized()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Não autorizado", ErrorType.Unauthorized);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            presenter.ObterResultado().Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact(DisplayName = "Deve retornar ObjectResult 403 quando ErrorType é NotAllowed")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornar403_QuandoErrorTypeNotAllowed()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Acesso negado", ErrorType.NotAllowed);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<ObjectResult>();
            (resultado as ObjectResult)!.StatusCode.Should().Be(403);
        }

        [Fact(DisplayName = "Deve retornar ObjectResult 500 quando ErrorType é desconhecido")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornar500_QuandoErrorTypeDesconhecido()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Erro inesperado", (ErrorType)999);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<ObjectResult>();
            (resultado as ObjectResult)!.StatusCode.Should().Be(500);
        }

        [Fact(DisplayName = "Deve retornar ObjectResult 500 para BadGateway")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornar500_QuandoErrorTypeBadGateway()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Falha de dependência", ErrorType.BadGateway);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<ObjectResult>();
            (resultado as ObjectResult)!.StatusCode.Should().Be(500);
        }

        [Fact(DisplayName = "Deve retornar ObjectResult 500 para UnexpectedError")]
        [Trait("Presenter", "BasePresenter")]
        public void ApresentarErro_DeveRetornar500_QuandoErrorTypeUnexpectedError()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            presenter.ApresentarErro("Erro inesperado", ErrorType.UnexpectedError);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<ObjectResult>();
            (resultado as ObjectResult)!.StatusCode.Should().Be(500);
        }

        #endregion

        #region ObterResultado

        [Fact(DisplayName = "Deve retornar StatusCodeResult 500 quando resultado não foi definido")]
        [Trait("Presenter", "BasePresenter")]
        public void ObterResultado_DeveRetornar500_QuandoResultadoNaoDefinido()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Act
            var resultado = presenter.ObterResultado();

            // Assert
            resultado.Should().BeOfType<StatusCodeResult>();
            (resultado as StatusCodeResult)!.StatusCode.Should().Be(500);
        }

        #endregion

        #region DefinirSucesso

        [Fact(DisplayName = "Deve definir sucesso com IActionResult")]
        [Trait("Presenter", "BasePresenter")]
        public void DefinirSucesso_DeveDefinir_QuandoChamadoComActionResult()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();
            var actionResult = new OkResult();

            // Act
            presenter.ChamarDefinirSucessoComActionResult(actionResult);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();
            presenter.ObterResultado().Should().Be(actionResult);
        }

        [Fact(DisplayName = "Deve definir sucesso com objeto retornando OkObjectResult")]
        [Trait("Presenter", "BasePresenter")]
        public void DefinirSucesso_DeveRetornarOk_QuandoChamadoComObjeto()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();
            var dados = new { id = 1, nome = "teste" };

            // Act
            presenter.ChamarDefinirSucessoComObjeto(dados);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();
            presenter.ObterResultado().Should().BeOfType<OkObjectResult>();
            var okResult = presenter.ObterResultado() as OkObjectResult;
            okResult!.Value.Should().Be(dados);
        }

        [Fact(DisplayName = "Deve definir sucesso com localização retornando CreatedAtActionResult")]
        [Trait("Presenter", "BasePresenter")]
        public void DefinirSucessoComLocalizacao_DeveRetornarCreatedAtAction_QuandoChamado()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();
            var dados = new { id = Guid.NewGuid() };

            // Act
            presenter.ChamarDefinirSucessoComLocalizacao("ObterPorId", "OrdemServico", new { id = dados.id }, dados);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();
            presenter.ObterResultado().Should().BeOfType<CreatedAtActionResult>();
        }

        #endregion

        #region FoiSucesso

        [Fact(DisplayName = "FoiSucesso deve ser false por padrão")]
        [Trait("Presenter", "BasePresenter")]
        public void FoiSucesso_DeveSerFalse_PorPadrao()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
        }

        [Fact(DisplayName = "FoiSucesso deve ser false após apresentar erro")]
        [Trait("Presenter", "BasePresenter")]
        public void FoiSucesso_DeveSerFalse_AposApresentarErro()
        {
            // Arrange
            var presenter = new BasePresenterConcreta();
            presenter.ChamarDefinirSucessoComObjeto(new { dado = "ok" });

            // Act
            presenter.ApresentarErro("Erro", ErrorType.InvalidInput);

            // Assert
            presenter.FoiSucesso.Should().BeFalse();
        }

        #endregion
    }
}
