using Application.Contracts.Presenters;
using Moq;
using Shared.Enums;

namespace Tests.Application.OrdemServico.Helpers
{
    /// <summary>
    /// Extensões fluentes de verificação para Mock de IObterTempoMedioPresenter.
    /// </summary>
    public static class ObterTempoMedioPresenterMockExtensions
    {
        public static void DeveTerApresentadoSucesso(this Mock<IObterTempoMedioPresenter> mock, int quantidadeDias, int quantidadeOrdensAnalisadas, double tempoMedioCompletoHoras, double tempoMedioExecucaoHoras)
        {
            mock.Verify(p => p.ApresentarSucesso(quantidadeDias, It.IsAny<DateTime>(), It.IsAny<DateTime>(), quantidadeOrdensAnalisadas, tempoMedioCompletoHoras, tempoMedioExecucaoHoras), Times.Once,
                $"Era esperado que o método ApresentarSucesso fosse chamado com quantidadeDias '{quantidadeDias}', quantidadeOrdensAnalisadas '{quantidadeOrdensAnalisadas}', tempoMedioCompleto '{tempoMedioCompletoHoras}' e tempoMedioExecucao '{tempoMedioExecucaoHoras}'.");
        }

        public static void DeveTerApresentadoSucessoComQualquerParametro(this Mock<IObterTempoMedioPresenter> mock)
        {
            mock.Verify(p => p.ApresentarSucesso(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Once,
                "Era esperado que o método ApresentarSucesso fosse chamado exatamente uma vez.");
        }

        public static void NaoDeveTerApresentadoSucesso(this Mock<IObterTempoMedioPresenter> mock)
        {
            mock.Verify(p => p.ApresentarSucesso(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never,
                "O método ApresentarSucesso não deveria ter sido chamado.");
        }

        public static void DeveTerApresentadoErro(this Mock<IObterTempoMedioPresenter> mock, string mensagem, ErrorType errorType)
        {
            mock.Verify(p => p.ApresentarErro(mensagem, errorType), Times.Once,
                $"Era esperado que o método ApresentarErro fosse chamado exatamente uma vez com a mensagem '{mensagem}' e tipo '{errorType}'.");
        }

        public static void NaoDeveTerApresentadoErro(this Mock<IObterTempoMedioPresenter> mock)
        {
            mock.Verify(p => p.ApresentarErro(It.IsAny<string>(), It.IsAny<ErrorType>()), Times.Never,
                "O método ApresentarErro não deveria ter sido chamado.");
        }
    }
}
