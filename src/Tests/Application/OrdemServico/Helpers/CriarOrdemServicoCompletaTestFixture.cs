using Application.Contracts.Gateways;
using Application.Contracts.Presenters;
using Application.OrdemServico.UseCases;
using Application.Contracts.Monitoramento;
using Application.OrdemServico.Interfaces.External;
using Moq;

namespace Tests.Application.OrdemServico.Helpers
{
    public class CriarOrdemServicoCompletaTestFixture
    {
        public Mock<IOrdemServicoGateway> OrdemServicoGatewayMock { get; }
        public Mock<IClienteExternalService> ClienteExternalServiceMock { get; }
        public Mock<IVeiculoExternalService> VeiculoExternalServiceMock { get; }
        public Mock<IServicoExternalService> ServicoExternalServiceMock { get; }
        public Mock<IEstoqueExternalService> EstoqueExternalServiceMock { get; }
        public Mock<ICriarOrdemServicoCompletaPresenter> PresenterMock { get; }
        public Mock<IMetricsService> MetricsServiceMock { get; }

        public CriarOrdemServicoCompletaUseCase UseCase { get; }

        public CriarOrdemServicoCompletaTestFixture()
        {
            OrdemServicoGatewayMock = new Mock<IOrdemServicoGateway>();
            ClienteExternalServiceMock = new Mock<IClienteExternalService>();
            VeiculoExternalServiceMock = new Mock<IVeiculoExternalService>();
            ServicoExternalServiceMock = new Mock<IServicoExternalService>();
            EstoqueExternalServiceMock = new Mock<IEstoqueExternalService>();
            PresenterMock = new Mock<ICriarOrdemServicoCompletaPresenter>();
            MetricsServiceMock = new Mock<IMetricsService>();

            UseCase = new CriarOrdemServicoCompletaUseCase();
        }
    }
}