using Application.Contracts.Messaging;
using System.Text.Json;
using Xunit;

namespace Tests.Infrastructure.Messaging.Contracts;

/// <summary>
/// Testes unitários para os contratos de mensageria relacionados à redução de estoque.
/// Valida serialização, deserialização e estrutura dos contratos.
/// </summary>
public class ReducaoEstoqueContractsTests
{
    [Fact]
    public void ReducaoEstoqueSolicitacao_WhenSerialized_ShouldContainAllFields()
    {
        // Arrange
        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Itens = new List<ItemReducao>
            {
                new ItemReducao { ItemEstoqueId = Guid.NewGuid(), Quantidade = 5 },
                new ItemReducao { ItemEstoqueId = Guid.NewGuid(), Quantidade = 3 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(solicitacao);
        var deserialized = JsonSerializer.Deserialize<ReducaoEstoqueSolicitacao>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(solicitacao.CorrelationId, deserialized.CorrelationId);
        Assert.Equal(solicitacao.OrdemServicoId, deserialized.OrdemServicoId);
        Assert.Equal(2, deserialized.Itens.Count);
        Assert.Equal(solicitacao.Itens[0].ItemEstoqueId, deserialized.Itens[0].ItemEstoqueId);
        Assert.Equal(solicitacao.Itens[0].Quantidade, deserialized.Itens[0].Quantidade);
    }

    [Fact]
    public void ReducaoEstoqueSolicitacao_WhenDeserializedFromJson_ShouldHaveCorrectValues()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();
        
        var json = @$"{{
            ""correlationId"": ""{correlationId}"",
            ""ordemServicoId"": ""{ordemServicoId}"",
            ""itens"": [
                {{
                    ""itemEstoqueId"": ""{itemId1}"",
                    ""quantidade"": 10
                }},
                {{
                    ""itemEstoqueId"": ""{itemId2}"",
                    ""quantidade"": 5
                }}
            ]
        }}";

        // Act
        var solicitacao = JsonSerializer.Deserialize<ReducaoEstoqueSolicitacao>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(solicitacao);
        Assert.Equal(correlationId, solicitacao.CorrelationId);
        Assert.Equal(ordemServicoId, solicitacao.OrdemServicoId);
        Assert.Equal(2, solicitacao.Itens.Count);
        Assert.Equal(itemId1, solicitacao.Itens[0].ItemEstoqueId);
        Assert.Equal(10, solicitacao.Itens[0].Quantidade);
        Assert.Equal(itemId2, solicitacao.Itens[1].ItemEstoqueId);
        Assert.Equal(5, solicitacao.Itens[1].Quantidade);
    }

    [Fact]
    public void ReducaoEstoqueResultado_WhenSerializedWithSucesso_ShouldContainAllFields()
    {
        // Arrange
        var resultado = new ReducaoEstoqueResultado
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Sucesso = true,
            MotivoFalha = null
        };

        // Act
        var json = JsonSerializer.Serialize(resultado);
        var deserialized = JsonSerializer.Deserialize<ReducaoEstoqueResultado>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(resultado.CorrelationId, deserialized.CorrelationId);
        Assert.Equal(resultado.OrdemServicoId, deserialized.OrdemServicoId);
        Assert.True(deserialized.Sucesso);
        Assert.Null(deserialized.MotivoFalha);
    }

    [Fact]
    public void ReducaoEstoqueResultado_WhenSerializedWithFalha_ShouldContainMotivoFalha()
    {
        // Arrange
        var resultado = new ReducaoEstoqueResultado
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Sucesso = false,
            MotivoFalha = "estoque_insuficiente"
        };

        // Act
        var json = JsonSerializer.Serialize(resultado);
        var deserialized = JsonSerializer.Deserialize<ReducaoEstoqueResultado>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(resultado.CorrelationId, deserialized.CorrelationId);
        Assert.Equal(resultado.OrdemServicoId, deserialized.OrdemServicoId);
        Assert.False(deserialized.Sucesso);
        Assert.Equal("estoque_insuficiente", deserialized.MotivoFalha);
    }

    [Fact]
    public void ReducaoEstoqueResultado_WhenDeserializedFromJson_ShouldHaveCorrectValues()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        
        var json = @$"{{
            ""correlationId"": ""{correlationId}"",
            ""ordemServicoId"": ""{ordemServicoId}"",
            ""sucesso"": false,
            ""motivoFalha"": ""erro_interno""
        }}";

        // Act
        var resultado = JsonSerializer.Deserialize<ReducaoEstoqueResultado>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(correlationId, resultado.CorrelationId);
        Assert.Equal(ordemServicoId, resultado.OrdemServicoId);
        Assert.False(resultado.Sucesso);
        Assert.Equal("erro_interno", resultado.MotivoFalha);
    }

    [Fact]
    public void ItemReducao_WhenSerialized_ShouldContainAllFields()
    {
        // Arrange
        var item = new ItemReducao
        {
            ItemEstoqueId = Guid.NewGuid(),
            Quantidade = 7
        };

        // Act
        var json = JsonSerializer.Serialize(item);
        var deserialized = JsonSerializer.Deserialize<ItemReducao>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(item.ItemEstoqueId, deserialized.ItemEstoqueId);
        Assert.Equal(item.Quantidade, deserialized.Quantidade);
    }

    [Theory]
    [InlineData("estoque_insuficiente")]
    [InlineData("erro_interno")]
    [InlineData("servico_indisponivel")]
    public void ReducaoEstoqueResultado_SupportsDifferentMotivosFalha(string motivoFalha)
    {
        // Arrange
        var resultado = new ReducaoEstoqueResultado
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Sucesso = false,
            MotivoFalha = motivoFalha
        };

        // Act
        var json = JsonSerializer.Serialize(resultado);
        var deserialized = JsonSerializer.Deserialize<ReducaoEstoqueResultado>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.False(deserialized.Sucesso);
        Assert.Equal(motivoFalha, deserialized.MotivoFalha);
    }

    [Fact]
    public void ReducaoEstoqueSolicitacao_WithEmptyItens_ShouldSerializeCorrectly()
    {
        // Arrange
        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            Itens = new List<ItemReducao>()
        };

        // Act
        var json = JsonSerializer.Serialize(solicitacao);
        var deserialized = JsonSerializer.Deserialize<ReducaoEstoqueSolicitacao>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Itens);
        Assert.Empty(deserialized.Itens);
    }
}
