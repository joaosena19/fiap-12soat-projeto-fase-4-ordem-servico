using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Moq;

namespace Tests.Application.SharedHelpers.ExternalServices
{
    public class VeiculoExternalServiceVerificarExistenciaSetupBuilder
    {
        private readonly Mock<IVeiculoExternalService> _mock;
        private readonly Guid _veiculoId;

        public VeiculoExternalServiceVerificarExistenciaSetupBuilder(Mock<IVeiculoExternalService> mock, Guid veiculoId)
        {
            _mock = mock;
            _veiculoId = veiculoId;
        }

        public void Retorna(bool existe) => _mock.Setup(s => s.VerificarExistenciaVeiculo(_veiculoId)).ReturnsAsync(existe);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.VerificarExistenciaVeiculo(_veiculoId)).ThrowsAsync(excecao);
    }

    public class VeiculoExternalServiceObterPorIdSetupBuilder
    {
        private readonly Mock<IVeiculoExternalService> _mock;
        private readonly Guid _veiculoId;

        public VeiculoExternalServiceObterPorIdSetupBuilder(Mock<IVeiculoExternalService> mock, Guid veiculoId)
        {
            _mock = mock;
            _veiculoId = veiculoId;
        }

        public void Retorna(VeiculoExternalDto veiculo) => _mock.Setup(s => s.ObterVeiculoPorIdAsync(_veiculoId)).ReturnsAsync(veiculo);

        public void NaoRetornaNada() => _mock.Setup(s => s.ObterVeiculoPorIdAsync(_veiculoId)).ReturnsAsync((VeiculoExternalDto?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.ObterVeiculoPorIdAsync(_veiculoId)).ThrowsAsync(excecao);
    }

    public class VeiculoExternalServiceObterPorPlacaSetupBuilder
    {
        private readonly Mock<IVeiculoExternalService> _mock;
        private readonly string _placa;

        public VeiculoExternalServiceObterPorPlacaSetupBuilder(Mock<IVeiculoExternalService> mock, string placa)
        {
            _mock = mock;
            _placa = placa;
        }

        public void Retorna(VeiculoExternalDto veiculo) => _mock.Setup(s => s.ObterVeiculoPorPlacaAsync(_placa)).ReturnsAsync(veiculo);

        public void NaoRetornaNada() => _mock.Setup(s => s.ObterVeiculoPorPlacaAsync(_placa)).ReturnsAsync((VeiculoExternalDto?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.ObterVeiculoPorPlacaAsync(_placa)).ThrowsAsync(excecao);
    }

    public class VeiculoExternalServiceCriarSetupBuilder
    {
        private readonly Mock<IVeiculoExternalService> _mock;

        public VeiculoExternalServiceCriarSetupBuilder(Mock<IVeiculoExternalService> mock)
        {
            _mock = mock;
        }

        public void Retorna(VeiculoExternalDto veiculo) => _mock.Setup(s => s.CriarVeiculoAsync(It.IsAny<CriarVeiculoExternalDto>())).ReturnsAsync(veiculo);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.CriarVeiculoAsync(It.IsAny<CriarVeiculoExternalDto>())).ThrowsAsync(excecao);
    }

    public static class VeiculoExternalServiceMockExtensions
    {
        public static VeiculoExternalServiceVerificarExistenciaSetupBuilder AoVerificarExistenciaVeiculo(this Mock<IVeiculoExternalService> mock, Guid veiculoId)
            => new VeiculoExternalServiceVerificarExistenciaSetupBuilder(mock, veiculoId);

        public static VeiculoExternalServiceVerificarExistenciaSetupBuilder AoVerificarExistencia(this Mock<IVeiculoExternalService> mock, Guid veiculoId)
            => new VeiculoExternalServiceVerificarExistenciaSetupBuilder(mock, veiculoId);

        public static VeiculoExternalServiceObterPorIdSetupBuilder AoObterPorId(this Mock<IVeiculoExternalService> mock, Guid veiculoId)
            => new VeiculoExternalServiceObterPorIdSetupBuilder(mock, veiculoId);

        public static VeiculoExternalServiceObterPorPlacaSetupBuilder AoObterPorPlaca(this Mock<IVeiculoExternalService> mock, string placa)
            => new VeiculoExternalServiceObterPorPlacaSetupBuilder(mock, placa);

        public static VeiculoExternalServiceCriarSetupBuilder AoCriar(this Mock<IVeiculoExternalService> mock)
            => new VeiculoExternalServiceCriarSetupBuilder(mock);
    }

    public static class VeiculoExternalServiceMockVerifyExtensions
    {
        public static void DeveTerVerificadoExistencia(this Mock<IVeiculoExternalService> mock, Guid veiculoId, int vezes = 1)
        {
            mock.Verify(x => x.VerificarExistenciaVeiculo(veiculoId), Times.Exactly(vezes));
        }

        public static void DeveTerObtidoPorId(this Mock<IVeiculoExternalService> mock, Guid veiculoId, int vezes = 1)
        {
            mock.Verify(x => x.ObterVeiculoPorIdAsync(veiculoId), Times.Exactly(vezes));
        }

        public static void DeveTerObtidoPorPlaca(this Mock<IVeiculoExternalService> mock, string placa, int vezes = 1)
        {
            mock.Verify(x => x.ObterVeiculoPorPlacaAsync(placa), Times.Exactly(vezes));
        }

        public static void DeveTerCriadoVeiculo(this Mock<IVeiculoExternalService> mock, int vezes = 1)
        {
            mock.Verify(x => x.CriarVeiculoAsync(It.IsAny<CriarVeiculoExternalDto>()), Times.Exactly(vezes));
        }

        public static void NaoDeveTerCriado(this Mock<IVeiculoExternalService> mock)
        {
            mock.Verify(x => x.CriarVeiculoAsync(It.IsAny<CriarVeiculoExternalDto>()), Times.Never);
        }
    }
}
