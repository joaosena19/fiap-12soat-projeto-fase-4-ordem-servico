using Application.OrdemServico.Interfaces.External;
using Moq;

namespace Tests.Integration.Mocks;

/// <summary>
/// Container de mocks para todos os serviços externos utilizados na aplicação.
/// Facilita o setup e reset de mocks entre testes de integração.
/// </summary>
public class MockExternalServices
{
    public Mock<IClienteExternalService> ClienteService { get; }
    public Mock<IVeiculoExternalService> VeiculoService { get; }
    public Mock<IServicoExternalService> ServicoService { get; }
    public Mock<IEstoqueExternalService> EstoqueService { get; }

    public MockExternalServices()
    {
        ClienteService = new Mock<IClienteExternalService>();
        VeiculoService = new Mock<IVeiculoExternalService>();
        ServicoService = new Mock<IServicoExternalService>();
        EstoqueService = new Mock<IEstoqueExternalService>();
    }

    /// <summary>
    /// Reseta todos os mocks para estado inicial.
    /// Útil para limpar setups entre testes.
    /// </summary>
    public void ResetAll()
    {
        ClienteService.Reset();
        VeiculoService.Reset();
        ServicoService.Reset();
        EstoqueService.Reset();
    }
}
