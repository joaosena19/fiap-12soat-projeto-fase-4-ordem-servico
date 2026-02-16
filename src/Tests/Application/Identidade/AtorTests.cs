using Application.Identidade.Services;
using FluentAssertions;
using Tests.Application.SharedHelpers;
using Xunit;

namespace Tests.Application.Identidade;

public class AtorTests
{
    #region EhCliente

    [Fact(DisplayName = "Deve retornar true quando role Cliente presente")]
    [Trait("Application", "Ator")]
    public void EhCliente_DeveRetornarTrue_QuandoRoleClientePresente()
    {
        // Arrange
        var ator = new AtorBuilder().ComoCliente(Guid.NewGuid()).Build();

        // Act
        var resultado = ator.EhCliente();

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando role Cliente ausente")]
    [Trait("Application", "Ator")]
    public void EhCliente_DeveRetornarFalse_QuandoRoleClienteAusente()
    {
        // Arrange
        var ator = new AtorBuilder().ComoAdministrador().Build();

        // Act
        var resultado = ator.EhCliente();

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion
}
