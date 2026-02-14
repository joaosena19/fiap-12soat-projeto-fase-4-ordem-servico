using FluentAssertions;
using Shared.Seed;

namespace Tests.Other.Seed;

public class SeedIdsTests
{
    #region Veiculos

    [Fact(DisplayName = "Deve manter GUID estável para Veiculo Abc1234")]
    [Trait("Other", "Seed")]
    public void Veiculos_Abc1234_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.Veiculos.Abc1234;

        // Assert
        guidAtual.Should().Be(Guid.Parse("3f8a2d3b-0d8b-4a3f-9b1e-7b65e6d2a901"));
    }

    [Fact(DisplayName = "Deve manter GUID estável para Veiculo Xyz5678")]
    [Trait("Other", "Seed")]
    public void Veiculos_Xyz5678_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.Veiculos.Xyz5678;

        // Assert
        guidAtual.Should().Be(Guid.Parse("0d2c5f44-6a50-4f8e-8d7a-0d6c7b0d1b2c"));
    }

    [Fact(DisplayName = "Deve manter GUID estável para Veiculo Def9012")]
    [Trait("Other", "Seed")]
    public void Veiculos_Def9012_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.Veiculos.Def9012;

        // Assert
        guidAtual.Should().Be(Guid.Parse("9b6d2a10-6a2f-4f7a-9e1b-2a3f0d8b3f8a"));
    }

    #endregion

    #region Servicos

    [Fact(DisplayName = "Deve manter GUID estável para Servico TrocaDeOleo")]
    [Trait("Other", "Seed")]
    public void Servicos_TrocaDeOleo_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.Servicos.TrocaDeOleo;

        // Assert
        guidAtual.Should().Be(Guid.Parse("1a111111-1111-1111-1111-111111111111"));
    }

    [Fact(DisplayName = "Deve manter GUID estável para Servico AlinhamentoBalanceamento")]
    [Trait("Other", "Seed")]
    public void Servicos_AlinhamentoBalanceamento_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.Servicos.AlinhamentoBalanceamento;

        // Assert
        guidAtual.Should().Be(Guid.Parse("2b222222-2222-2222-2222-222222222222"));
    }

    [Fact(DisplayName = "Deve manter GUID estável para Servico RevisaoCompleta")]
    [Trait("Other", "Seed")]
    public void Servicos_RevisaoCompleta_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.Servicos.RevisaoCompleta;

        // Assert
        guidAtual.Should().Be(Guid.Parse("3c333333-3333-3333-3333-333333333333"));
    }

    #endregion

    #region ItensEstoque

    [Fact(DisplayName = "Deve manter GUID estável para ItemEstoque OleoMotor5w30")]
    [Trait("Other", "Seed")]
    public void ItensEstoque_OleoMotor5w30_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.ItensEstoque.OleoMotor5w30;

        // Assert
        guidAtual.Should().Be(Guid.Parse("4d444444-4444-4444-4444-444444444444"));
    }

    [Fact(DisplayName = "Deve manter GUID estável para ItemEstoque FiltroDeOleo")]
    [Trait("Other", "Seed")]
    public void ItensEstoque_FiltroDeOleo_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.ItensEstoque.FiltroDeOleo;

        // Assert
        guidAtual.Should().Be(Guid.Parse("5e555555-5555-5555-5555-555555555555"));
    }

    [Fact(DisplayName = "Deve manter GUID estável para ItemEstoque PastilhaDeFreioDianteira")]
    [Trait("Other", "Seed")]
    public void ItensEstoque_PastilhaDeFreioDianteira_DeveManterGuidEstavel()
    {
        // Arrange & Act
        var guidAtual = SeedIds.ItensEstoque.PastilhaDeFreioDianteira;

        // Assert
        guidAtual.Should().Be(Guid.Parse("6f666666-6666-6666-6666-666666666666"));
    }

    #endregion

    #region Agregacao

    [Fact(DisplayName = "Deve conter todos os GUIDs esperados quando validados em conjunto")]
    [Trait("Other", "Seed")]
    public void SeedIds_DeveConterGuidsEsperados_QuandoDefinidos()
    {
        // Arrange
        var guidsVeiculos = new[]
        {
            SeedIds.Veiculos.Abc1234,
            SeedIds.Veiculos.Xyz5678,
            SeedIds.Veiculos.Def9012
        };

        var guidsServicos = new[]
        {
            SeedIds.Servicos.TrocaDeOleo,
            SeedIds.Servicos.AlinhamentoBalanceamento,
            SeedIds.Servicos.RevisaoCompleta
        };

        var guidsItensEstoque = new[]
        {
            SeedIds.ItensEstoque.OleoMotor5w30,
            SeedIds.ItensEstoque.FiltroDeOleo,
            SeedIds.ItensEstoque.PastilhaDeFreioDianteira
        };

        // Act & Assert
        guidsVeiculos.Should().HaveCount(3);
        guidsVeiculos.Should().OnlyHaveUniqueItems();
        guidsVeiculos.Should().NotContain(Guid.Empty);

        guidsServicos.Should().HaveCount(3);
        guidsServicos.Should().OnlyHaveUniqueItems();
        guidsServicos.Should().NotContain(Guid.Empty);

        guidsItensEstoque.Should().HaveCount(3);
        guidsItensEstoque.Should().OnlyHaveUniqueItems();
        guidsItensEstoque.Should().NotContain(Guid.Empty);

        // Verifica que não há colisões entre diferentes categorias
        var todosGuids = guidsVeiculos.Concat(guidsServicos).Concat(guidsItensEstoque);
        todosGuids.Should().HaveCount(9);
        todosGuids.Should().OnlyHaveUniqueItems();
    }

    #endregion
}
