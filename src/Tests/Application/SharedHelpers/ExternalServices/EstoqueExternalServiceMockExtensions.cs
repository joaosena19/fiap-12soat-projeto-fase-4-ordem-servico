using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using Moq;

namespace Tests.Application.SharedHelpers.ExternalServices
{
    public class EstoqueExternalServiceObterPorIdSetupBuilder
    {
        private readonly Mock<IEstoqueExternalService> _mock;
        private readonly Guid _itemId;

        public EstoqueExternalServiceObterPorIdSetupBuilder(Mock<IEstoqueExternalService> mock, Guid itemId)
        {
            _mock = mock;
            _itemId = itemId;
        }

        public void Retorna(ItemEstoqueExternalDto itemEstoque) => _mock.Setup(s => s.ObterItemEstoquePorIdAsync(_itemId)).ReturnsAsync(itemEstoque);

        public void NaoRetornaNada() => _mock.Setup(s => s.ObterItemEstoquePorIdAsync(_itemId)).ReturnsAsync((ItemEstoqueExternalDto?)null);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.ObterItemEstoquePorIdAsync(_itemId)).ThrowsAsync(excecao);
    }

    public class EstoqueExternalServiceVerificarDisponibilidadeSetupBuilder
    {
        private readonly Mock<IEstoqueExternalService> _mock;
        private readonly Guid _itemId;
        private readonly int _quantidade;

        public EstoqueExternalServiceVerificarDisponibilidadeSetupBuilder(Mock<IEstoqueExternalService> mock, Guid itemId, int quantidade)
        {
            _mock = mock;
            _itemId = itemId;
            _quantidade = quantidade;
        }

        public void Retorna(bool disponivel) => _mock.Setup(s => s.VerificarDisponibilidadeAsync(_itemId, _quantidade)).ReturnsAsync(disponivel);

        public void LancaExcecao(Exception excecao) => _mock.Setup(s => s.VerificarDisponibilidadeAsync(_itemId, _quantidade)).ThrowsAsync(excecao);
    }



    public static class EstoqueExternalServiceMockExtensions
    {
        public static EstoqueExternalServiceObterPorIdSetupBuilder AoObterPorId(this Mock<IEstoqueExternalService> mock, Guid itemId)
            => new EstoqueExternalServiceObterPorIdSetupBuilder(mock, itemId);

        public static EstoqueExternalServiceObterPorIdSetupBuilder AoObterItemEstoquePorId(this Mock<IEstoqueExternalService> mock, Guid itemId)
            => new EstoqueExternalServiceObterPorIdSetupBuilder(mock, itemId);

        public static EstoqueExternalServiceVerificarDisponibilidadeSetupBuilder AoVerificarDisponibilidade(this Mock<IEstoqueExternalService> mock, Guid itemId, int quantidade)
            => new EstoqueExternalServiceVerificarDisponibilidadeSetupBuilder(mock, itemId, quantidade);


    }

    public static class EstoqueExternalServiceMockVerifyExtensions
    {
        public static void DeveTerObtidoPorId(this Mock<IEstoqueExternalService> mock, Guid itemId, int vezes = 1)
        {
            mock.Verify(x => x.ObterItemEstoquePorIdAsync(itemId), Times.Exactly(vezes));
        }

        public static void DeveTerVerificadoDisponibilidade(this Mock<IEstoqueExternalService> mock, Guid itemId, int quantidadeNecessaria, int vezes = 1)
        {
            mock.Verify(x => x.VerificarDisponibilidadeAsync(itemId, quantidadeNecessaria), Times.Exactly(vezes));
        }


    }
}
