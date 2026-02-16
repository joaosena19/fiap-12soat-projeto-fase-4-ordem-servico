using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Moq;

namespace Tests.Application.SharedHelpers.ExternalServices
{
    public class ClienteExternalServiceObterPorVeiculoIdSetupBuilder
    {
        private readonly Mock<IClienteExternalService> _mock;
        private readonly Guid _veiculoId;

        public ClienteExternalServiceObterPorVeiculoIdSetupBuilder(Mock<IClienteExternalService> mock, Guid veiculoId)
        {
            _mock = mock;
            _veiculoId = veiculoId;
        }

        public void Retorna(ClienteExternalDto cliente) => _mock.Setup(s => s.ObterClientePorVeiculoIdAsync(_veiculoId)).ReturnsAsync(cliente);

        public void NaoRetornaNada() => _mock.Setup(s => s.ObterClientePorVeiculoIdAsync(_veiculoId)).ReturnsAsync((ClienteExternalDto?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.ObterClientePorVeiculoIdAsync(_veiculoId)).ThrowsAsync(excecao);
    }

    public class ClienteExternalServiceObterPorDocumentoSetupBuilder
    {
        private readonly Mock<IClienteExternalService> _mock;
        private readonly string _documento;

        public ClienteExternalServiceObterPorDocumentoSetupBuilder(Mock<IClienteExternalService> mock, string documento)
        {
            _mock = mock;
            _documento = documento;
        }

        public void Retorna(ClienteExternalDto cliente) => _mock.Setup(s => s.ObterPorDocumentoAsync(_documento)).ReturnsAsync(cliente);

        public void NaoRetornaNada() => _mock.Setup(s => s.ObterPorDocumentoAsync(_documento)).ReturnsAsync((ClienteExternalDto?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.ObterPorDocumentoAsync(_documento)).ThrowsAsync(excecao);
    }

    public class ClienteExternalServiceCriarSetupBuilder
    {
        private readonly Mock<IClienteExternalService> _mock;

        public ClienteExternalServiceCriarSetupBuilder(Mock<IClienteExternalService> mock)
        {
            _mock = mock;
        }

        public void Retorna(ClienteExternalDto cliente) => _mock.Setup(s => s.CriarClienteAsync(It.IsAny<CriarClienteExternalDto>())).ReturnsAsync(cliente);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.CriarClienteAsync(It.IsAny<CriarClienteExternalDto>())).ThrowsAsync(excecao);
    }

    public static class ClienteExternalServiceMockExtensions
    {
        public static ClienteExternalServiceObterPorVeiculoIdSetupBuilder AoObterClientePorVeiculoId(this Mock<IClienteExternalService> mock, Guid veiculoId)
            => new ClienteExternalServiceObterPorVeiculoIdSetupBuilder(mock, veiculoId);

        public static ClienteExternalServiceObterPorVeiculoIdSetupBuilder AoObterPorVeiculoId(this Mock<IClienteExternalService> mock, Guid veiculoId)
            => new ClienteExternalServiceObterPorVeiculoIdSetupBuilder(mock, veiculoId);

        public static ClienteExternalServiceObterPorDocumentoSetupBuilder AoObterPorDocumento(this Mock<IClienteExternalService> mock, string documento)
            => new ClienteExternalServiceObterPorDocumentoSetupBuilder(mock, documento);

        public static ClienteExternalServiceCriarSetupBuilder AoCriar(this Mock<IClienteExternalService> mock)
            => new ClienteExternalServiceCriarSetupBuilder(mock);
    }

    public static class ClienteExternalServiceMockVerifyExtensions
    {
        public static void DeveTerObtidoPorVeiculoId(this Mock<IClienteExternalService> mock, Guid veiculoId, int vezes = 1)
        {
            mock.Verify(x => x.ObterClientePorVeiculoIdAsync(veiculoId), Times.Exactly(vezes));
        }

        public static void DeveTerObtidoPorDocumento(this Mock<IClienteExternalService> mock, string documento, int vezes = 1)
        {
            mock.Verify(x => x.ObterPorDocumentoAsync(documento), Times.Exactly(vezes));
        }

        public static void DeveTerCriadoCliente(this Mock<IClienteExternalService> mock, int vezes = 1)
        {
            mock.Verify(x => x.CriarClienteAsync(It.IsAny<CriarClienteExternalDto>()), Times.Exactly(vezes));
        }

        public static void NaoDeveTerCriado(this Mock<IClienteExternalService> mock)
        {
            mock.Verify(x => x.CriarClienteAsync(It.IsAny<CriarClienteExternalDto>()), Times.Never);
        }
    }
}
