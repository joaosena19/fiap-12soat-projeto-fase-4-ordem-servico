using Application.Contracts.Gateways;
using Moq;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Application.SharedHelpers.Gateways
{
    public class OrdemServicoGatewayObterPorIdSetupBuilder
    {
        private readonly Mock<IOrdemServicoGateway> _mock;
        private readonly Guid _id;

        public OrdemServicoGatewayObterPorIdSetupBuilder(Mock<IOrdemServicoGateway> mock, Guid id)
        {
            _mock = mock;
            _id = id;
        }

        public void Retorna(OrdemServicoAggregate ordemServico) => _mock.Setup(g => g.ObterPorIdAsync(_id)).ReturnsAsync(ordemServico);

        public void NaoRetornaNada() => _mock.Setup(g => g.ObterPorIdAsync(_id)).ReturnsAsync((OrdemServicoAggregate?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(g => g.ObterPorIdAsync(_id)).ThrowsAsync(excecao);
    }

    public class OrdemServicoGatewayObterPorCodigoSetupBuilder
    {
        private readonly Mock<IOrdemServicoGateway> _mock;
        private readonly string _codigo;

        public OrdemServicoGatewayObterPorCodigoSetupBuilder(Mock<IOrdemServicoGateway> mock, string codigo)
        {
            _mock = mock;
            _codigo = codigo;
        }

        public void Retorna(OrdemServicoAggregate ordemServico) => _mock.Setup(g => g.ObterPorCodigoAsync(_codigo)).ReturnsAsync(ordemServico);

        public void NaoRetornaNada() => _mock.Setup(g => g.ObterPorCodigoAsync(_codigo)).ReturnsAsync((OrdemServicoAggregate?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(g => g.ObterPorCodigoAsync(_codigo)).ThrowsAsync(excecao);
    }

    public class OrdemServicoGatewayObterTodosSetupBuilder
    {
        private readonly Mock<IOrdemServicoGateway> _mock;

        public OrdemServicoGatewayObterTodosSetupBuilder(Mock<IOrdemServicoGateway> mock)
        {
            _mock = mock;
        }

        public void Retorna(IEnumerable<OrdemServicoAggregate> ordensServico) => _mock.Setup(g => g.ObterTodosAsync()).ReturnsAsync(ordensServico);

        public void RetornaListaVazia() => _mock.Setup(g => g.ObterTodosAsync()).ReturnsAsync(new List<OrdemServicoAggregate>());

        public void LancaExcecao(Exception excecao) => _mock.Setup(g => g.ObterTodosAsync()).ThrowsAsync(excecao);
    }

    public class OrdemServicoGatewayAtualizarSetupBuilder
    {
        private readonly Mock<IOrdemServicoGateway> _mock;

        public OrdemServicoGatewayAtualizarSetupBuilder(Mock<IOrdemServicoGateway> mock)
        {
            _mock = mock;
        }

        public void Retorna(OrdemServicoAggregate ordemServico) => _mock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ReturnsAsync(ordemServico);

        public void ComCallback(Action<OrdemServicoAggregate> callback) => _mock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).Callback(callback).ReturnsAsync((OrdemServicoAggregate os) => os);

        public void LancaExcecao(Exception excecao) => _mock.Setup(g => g.AtualizarAsync(It.IsAny<OrdemServicoAggregate>())).ThrowsAsync(excecao);
    }

    public class OrdemServicoGatewaySalvarSetupBuilder
    {
        private readonly Mock<IOrdemServicoGateway> _mock;

        public OrdemServicoGatewaySalvarSetupBuilder(Mock<IOrdemServicoGateway> mock)
        {
            _mock = mock;
        }

        public void Retorna(OrdemServicoAggregate ordemServico) => _mock.Setup(g => g.SalvarAsync(It.IsAny<OrdemServicoAggregate>())).ReturnsAsync(ordemServico);

        public void ComCallback(Action<OrdemServicoAggregate> callback) => _mock.Setup(g => g.SalvarAsync(It.IsAny<OrdemServicoAggregate>())).Callback(callback).ReturnsAsync((OrdemServicoAggregate os) => os);

        public void LancaExcecao(Exception excecao) => _mock.Setup(g => g.SalvarAsync(It.IsAny<OrdemServicoAggregate>())).ThrowsAsync(excecao);
    }

    public class OrdemServicoGatewayObterEntreguesUltimosDiasSetupBuilder
    {
        private readonly Mock<IOrdemServicoGateway> _mock;
        private readonly int _quantidadeDias;

        public OrdemServicoGatewayObterEntreguesUltimosDiasSetupBuilder(Mock<IOrdemServicoGateway> mock, int quantidadeDias)
        {
            _mock = mock;
            _quantidadeDias = quantidadeDias;
        }

        public void Retorna(IEnumerable<OrdemServicoAggregate> ordensEntregues) => _mock.Setup(g => g.ObterEntreguesUltimosDiasAsync(_quantidadeDias)).ReturnsAsync(ordensEntregues);

        public void RetornaListaVazia() => _mock.Setup(g => g.ObterEntreguesUltimosDiasAsync(_quantidadeDias)).ReturnsAsync(new List<OrdemServicoAggregate>());

        public void LancaExcecao(Exception excecao) => _mock.Setup(g => g.ObterEntreguesUltimosDiasAsync(_quantidadeDias)).ThrowsAsync(excecao);
    }

    public static class OrdemServicoGatewayMockExtensions
    {
        public static OrdemServicoGatewayObterPorIdSetupBuilder AoObterPorId(this Mock<IOrdemServicoGateway> mock, Guid id) => new OrdemServicoGatewayObterPorIdSetupBuilder(mock, id);

        public static OrdemServicoGatewayObterPorCodigoSetupBuilder AoObterPorCodigo(this Mock<IOrdemServicoGateway> mock, string codigo) => new OrdemServicoGatewayObterPorCodigoSetupBuilder(mock, codigo);

        public static OrdemServicoGatewayObterTodosSetupBuilder AoObterTodos(this Mock<IOrdemServicoGateway> mock) => new OrdemServicoGatewayObterTodosSetupBuilder(mock);

        public static OrdemServicoGatewayAtualizarSetupBuilder AoAtualizar(this Mock<IOrdemServicoGateway> mock) => new OrdemServicoGatewayAtualizarSetupBuilder(mock);

        public static OrdemServicoGatewaySalvarSetupBuilder AoSalvar(this Mock<IOrdemServicoGateway> mock) => new OrdemServicoGatewaySalvarSetupBuilder(mock);

        public static OrdemServicoGatewayObterEntreguesUltimosDiasSetupBuilder AoObterEntreguesUltimosDias(this Mock<IOrdemServicoGateway> mock, int quantidadeDias) => new OrdemServicoGatewayObterEntreguesUltimosDiasSetupBuilder(mock, quantidadeDias);
    }

    public static class OrdemServicoGatewayMockVerifyExtensions
    {
        public static void DeveTerVerificadoCodigoExistente(this Mock<IOrdemServicoGateway> mock)
        {
            mock.Verify(g => g.ObterPorCodigoAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        public static void DeveTerSalvoOrdemServico(this Mock<IOrdemServicoGateway> mock)
        {
            mock.Verify(g => g.SalvarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Once);
        }

        public static void DeveTerObtidoOrdemServicoPorCodigo(this Mock<IOrdemServicoGateway> mock, int vezes = 1)
        {
            mock.Verify(g => g.ObterPorCodigoAsync(It.IsAny<string>()), Times.Exactly(vezes));
        }

        public static void DeveTerSalvadoOrdemServico(this Mock<IOrdemServicoGateway> mock, int vezes = 1)
        {
            mock.Verify(g => g.SalvarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Exactly(vezes));
        }

        public static void NaoDeveTerSalvoOrdemServico(this Mock<IOrdemServicoGateway> mock)
        {
            mock.Verify(g => g.SalvarAsync(It.IsAny<OrdemServicoAggregate>()), Times.Never);
        }
    }
}