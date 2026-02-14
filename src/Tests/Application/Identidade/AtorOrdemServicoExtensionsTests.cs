using Application.Contracts.Gateways;
using Application.Identidade.Services.Extensions;
using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;
using FluentAssertions;
using Moq;
using Tests.Application.SharedHelpers;
using Tests.Application.SharedHelpers.AggregateBuilders;
using Tests.Application.SharedHelpers.ExternalServices;
using Tests.Application.SharedHelpers.Gateways;
using Xunit;

namespace Tests.Application.Identidade;

public class AtorOrdemServicoExtensionsTests
{
    #region PodeAcessarOrdemServicoAsync (com agregado)

    [Fact(DisplayName = "Deve retornar true quando administrador sem verificar veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsync_DeveRetornarTrue_QuandoAdministrador()
    {
        // Arrange
        var ator = new AtorBuilder().ComoAdministrador().Build();
        var ordemServico = new OrdemServicoBuilder().Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeTrue();
        veiculoServiceMock.Verify(x => x.ObterVeiculoPorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "Deve retornar true quando cliente é dono do veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsync_DeveRetornarTrue_QuandoClienteEhDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = clienteId };
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeTrue();
        veiculoServiceMock.DeveTerObtidoPorId(veiculoId);
    }

    [Fact(DisplayName = "Deve retornar false quando cliente não é dono do veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsync_DeveRetornarFalse_QuandoClienteNaoEhDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var outroClienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = outroClienteId };
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar false quando veículo não existe")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsync_DeveRetornarFalse_QuandoVeiculoNaoExiste()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        veiculoServiceMock.AoObterPorId(veiculoId).NaoRetornaNada();

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion

    #region PodeAcessarOrdemServicoAsync (por Id)

    [Fact(DisplayName = "Deve retornar true quando administrador sem verificar ordem")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsyncPorId_DeveRetornarTrue_QuandoAdministrador()
    {
        // Arrange
        var ator = new AtorBuilder().ComoAdministrador().Build();
        var ordemServicoId = Guid.NewGuid();
        var ordemServicoGatewayMock = new Mock<IOrdemServicoGateway>();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServicoId, ordemServicoGatewayMock.Object, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeTrue();
        ordemServicoGatewayMock.Verify(x => x.ObterPorIdAsync(It.IsAny<Guid>()), Times.Never);
        veiculoServiceMock.Verify(x => x.ObterVeiculoPorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "Deve retornar false quando ordem de serviço não existir")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsyncPorId_DeveRetornarFalse_QuandoOrdemServicoNaoExistir()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServicoGatewayMock = new Mock<IOrdemServicoGateway>();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        ordemServicoGatewayMock.AoObterPorId(ordemServicoId).NaoRetornaNada();

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServicoId, ordemServicoGatewayMock.Object, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar true quando cliente é dono do veículo da ordem")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsyncPorId_DeveRetornarTrue_QuandoClienteEhDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var ordemServicoGatewayMock = new Mock<IOrdemServicoGateway>();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = clienteId };
        ordemServicoGatewayMock.AoObterPorId(ordemServicoId).Retorna(ordemServico);
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServicoId, ordemServicoGatewayMock.Object, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando cliente não é dono do veículo da ordem")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAcessarOrdemServicoAsyncPorId_DeveRetornarFalse_QuandoClienteNaoEhDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var outroClienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var ordemServicoGatewayMock = new Mock<IOrdemServicoGateway>();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = outroClienteId };
        ordemServicoGatewayMock.AoObterPorId(ordemServicoId).Retorna(ordemServico);
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await ator.PodeAcessarOrdemServicoAsync(ordemServicoId, ordemServicoGatewayMock.Object, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion

    #region PodeCriarOrdemServicoParaVeiculoAsync

    [Fact(DisplayName = "Deve retornar true quando administrador sem verificar veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeCriarOrdemServicoParaVeiculoAsync_DeveRetornarTrue_QuandoAdministrador()
    {
        // Arrange
        var ator = new AtorBuilder().ComoAdministrador().Build();
        var veiculoId = Guid.NewGuid();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();

        // Act
        var resultado = await ator.PodeCriarOrdemServicoParaVeiculoAsync(veiculoId, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeTrue();
        veiculoServiceMock.Verify(x => x.ObterVeiculoPorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "Deve retornar false quando veículo não existir")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeCriarOrdemServicoParaVeiculoAsync_DeveRetornarFalse_QuandoVeiculoNaoExistir()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        veiculoServiceMock.AoObterPorId(veiculoId).NaoRetornaNada();

        // Act
        var resultado = await ator.PodeCriarOrdemServicoParaVeiculoAsync(veiculoId, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar true quando ator for dono do veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeCriarOrdemServicoParaVeiculoAsync_DeveRetornarTrue_QuandoAtorForDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = clienteId };
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await ator.PodeCriarOrdemServicoParaVeiculoAsync(veiculoId, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando ator não for dono do veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeCriarOrdemServicoParaVeiculoAsync_DeveRetornarFalse_QuandoAtorNaoForDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var outroClienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ator = new AtorBuilder().ComoCliente(clienteId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = outroClienteId };
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await ator.PodeCriarOrdemServicoParaVeiculoAsync(veiculoId, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion

    #region PodeAtualizarStatusOrdem

    [Fact(DisplayName = "Deve retornar true quando administrador ou sistema")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public void PodeAtualizarStatusOrdem_DeveRetornarTrue_QuandoAdministradorOuSistema()
    {
        // Arrange
        var administrador = new AtorBuilder().ComoAdministrador().Build();
        var sistema = new AtorBuilder().ComoSistema().Build();

        // Act
        var resultadoAdmin = administrador.PodeAtualizarStatusOrdem();
        var resultadoSistema = sistema.PodeAtualizarStatusOrdem();

        // Assert
        resultadoAdmin.Should().BeTrue();
        resultadoSistema.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando cliente")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public void PodeAtualizarStatusOrdem_DeveRetornarFalse_QuandoCliente()
    {
        // Arrange
        var cliente = new AtorBuilder().ComoCliente(Guid.NewGuid()).Build();

        // Act
        var resultado = cliente.PodeAtualizarStatusOrdem();

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion

    #region PodeGerenciarOrdemServico

    [Fact(DisplayName = "Deve retornar true quando administrador")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public void PodeGerenciarOrdemServico_DeveRetornarTrue_QuandoAdministrador()
    {
        // Arrange
        var administrador = new AtorBuilder().ComoAdministrador().Build();

        // Act
        var resultado = administrador.PodeGerenciarOrdemServico();

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando cliente ou sistema")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public void PodeGerenciarOrdemServico_DeveRetornarFalse_QuandoClienteOuSistema()
    {
        // Arrange
        var cliente = new AtorBuilder().ComoCliente(Guid.NewGuid()).Build();
        var sistema = new AtorBuilder().ComoSistema().Build();

        // Act
        var resultadoCliente = cliente.PodeGerenciarOrdemServico();
        var resultadoSistema = sistema.PodeGerenciarOrdemServico();

        // Assert
        resultadoCliente.Should().BeFalse();
        resultadoSistema.Should().BeFalse();
    }

    #endregion

    #region PodeAprovarDesaprovarOrcamento

    [Fact(DisplayName = "Deve retornar true quando administrador ou sistema")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAprovarDesaprovarOrcamento_DeveRetornarTrue_QuandoAdministradorOuSistema()
    {
        // Arrange
        var administrador = new AtorBuilder().ComoAdministrador().Build();
        var sistema = new AtorBuilder().ComoSistema().Build();
        var ordemServico = new OrdemServicoBuilder().Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();

        // Act
        var resultadoAdmin = await administrador.PodeAprovarDesaprovarOrcamento(ordemServico, veiculoServiceMock.Object);
        var resultadoSistema = await sistema.PodeAprovarDesaprovarOrcamento(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultadoAdmin.Should().BeTrue();
        resultadoSistema.Should().BeTrue();
        veiculoServiceMock.Verify(x => x.ObterVeiculoPorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "Deve retornar true quando cliente é dono do veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAprovarDesaprovarOrcamento_DeveRetornarTrue_QuandoClienteEhDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var cliente = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = clienteId };
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await cliente.PodeAprovarDesaprovarOrcamento(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando cliente não é dono do veículo")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAprovarDesaprovarOrcamento_DeveRetornarFalse_QuandoClienteNaoEhDonoDoVeiculo()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var outroClienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var cliente = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        var veiculo = new VeiculoExternalDto { Id = veiculoId, ClienteId = outroClienteId };
        veiculoServiceMock.AoObterPorId(veiculoId).Retorna(veiculo);

        // Act
        var resultado = await cliente.PodeAprovarDesaprovarOrcamento(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar false quando veículo não existe")]
    [Trait("Application", "AtorOrdemServicoExtensions")]
    public async Task PodeAprovarDesaprovarOrcamento_DeveRetornarFalse_QuandoVeiculoNaoExiste()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var cliente = new AtorBuilder().ComoCliente(clienteId).Build();
        var ordemServico = new OrdemServicoBuilder().ComVeiculoId(veiculoId).Build();
        var veiculoServiceMock = new Mock<IVeiculoExternalService>();
        veiculoServiceMock.AoObterPorId(veiculoId).NaoRetornaNada();

        // Act
        var resultado = await cliente.PodeAprovarDesaprovarOrcamento(ordemServico, veiculoServiceMock.Object);

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion
}
