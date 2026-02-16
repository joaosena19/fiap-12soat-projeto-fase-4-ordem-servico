using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Moq;

namespace Tests.Application.SharedHelpers.ExternalServices
{
    public class ServicoExternalServiceObterPorIdSetupBuilder
    {
        private readonly Mock<IServicoExternalService> _mock;
        private readonly Guid _servicoId;

        public ServicoExternalServiceObterPorIdSetupBuilder(Mock<IServicoExternalService> mock, Guid servicoId)
        {
            _mock = mock;
            _servicoId = servicoId;
        }

        public void Retorna(ServicoExternalDto servico) => _mock.Setup(s => s.ObterServicoPorIdAsync(_servicoId)).ReturnsAsync(servico);

        public void NaoRetornaNada() => _mock.Setup(s => s.ObterServicoPorIdAsync(_servicoId)).ReturnsAsync((ServicoExternalDto?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.ObterServicoPorIdAsync(_servicoId)).ThrowsAsync(excecao);
    }

    public static class ServicoExternalServiceMockExtensions
    {
        public static ServicoExternalServiceObterPorIdSetupBuilder AoObterPorId(this Mock<IServicoExternalService> mock, Guid servicoId)
            => new ServicoExternalServiceObterPorIdSetupBuilder(mock, servicoId);

        public static ServicoExternalServiceObterPorIdSetupBuilder AoObterServicoPorId(this Mock<IServicoExternalService> mock, Guid servicoId)
            => new ServicoExternalServiceObterPorIdSetupBuilder(mock, servicoId);
    }

    public static class ServicoExternalServiceMockVerifyExtensions
    {
        public static void DeveTerObtidoPorId(this Mock<IServicoExternalService> mock, Guid servicoId, int vezes = 1)
        {
            mock.Verify(x => x.ObterServicoPorIdAsync(servicoId), Times.Exactly(vezes));
        }

        public static void NaoDeveTerObtidoPorId(this Mock<IServicoExternalService> mock)
        {
            mock.Verify(x => x.ObterServicoPorIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
