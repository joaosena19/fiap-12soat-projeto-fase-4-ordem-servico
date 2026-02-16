using Application.Contracts.Presenters;
using Moq;
using Shared.Enums;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Application.OrdemServico.Helpers
{
    /// <summary>
    /// Extensões fluentes de verificação para Mock de IGerarOrcamentoPresenter.
    /// </summary>
    public static class GerarOrcamentoPresenterMockExtensions
    {
        public static void DeveTerApresentadoSucesso(this Mock<IGerarOrcamentoPresenter> mock, OrdemServicoAggregate ordemServico)
        {
            mock.Verify(p => p.ApresentarSucesso(ordemServico), Times.Once,
                "Era esperado que o método ApresentarSucesso fosse chamado exatamente uma vez com a ordem de serviço fornecida.");
        }

        public static void DeveTerApresentadoSucessoComQualquerObjeto(this Mock<IGerarOrcamentoPresenter> mock)
        {
            mock.Verify(p => p.ApresentarSucesso(It.IsAny<OrdemServicoAggregate>()), Times.Once,
                "Era esperado que o método ApresentarSucesso fosse chamado exatamente uma vez.");
        }

        public static void NaoDeveTerApresentadoSucesso(this Mock<IGerarOrcamentoPresenter> mock)
        {
            mock.Verify(p => p.ApresentarSucesso(It.IsAny<OrdemServicoAggregate>()), Times.Never,
                "O método ApresentarSucesso não deveria ter sido chamado.");
        }

        public static void DeveTerApresentadoErro(this Mock<IGerarOrcamentoPresenter> mock, string mensagem, ErrorType errorType)
        {
            mock.Verify(p => p.ApresentarErro(mensagem, errorType), Times.Once,
                $"Era esperado que o método ApresentarErro fosse chamado exatamente uma vez com a mensagem '{mensagem}' e tipo '{errorType}'.");
        }

        public static void DeveTerApresentadoErroComTipo(this Mock<IGerarOrcamentoPresenter> mock, ErrorType errorType)
        {
            mock.Verify(p => p.ApresentarErro(It.IsAny<string>(), errorType), Times.Once,
                $"Era esperado que o método ApresentarErro fosse chamado exatamente uma vez com qualquer mensagem e tipo '{errorType}'.");
        }

        public static void NaoDeveTerApresentadoErro(this Mock<IGerarOrcamentoPresenter> mock)
        {
            mock.Verify(p => p.ApresentarErro(It.IsAny<string>(), It.IsAny<ErrorType>()), Times.Never,
                "O método ApresentarErro não deveria ter sido chamado.");
        }
    }
}
