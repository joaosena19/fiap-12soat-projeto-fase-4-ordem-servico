using Application.Contracts.Messaging.DTOs;
using FluentAssertions;
using System.Text.Json;

namespace Tests.Application.Contracts.Messaging.DTOs;

/// <summary>
/// Testes unitários para os contratos de mensageria relacionados à redução de estoque.
/// Valida serialização, deserialização e estrutura dos contratos.
/// </summary>
public class ReducaoEstoqueContractsTests
{
    [Fact(DisplayName = "Deve conter todos os campos ao serializar ReducaoEstoqueSolicitacao")]
    [Trait("Contrato", "ReducaoEstoque")]
    public void Serializar_DeveConterTodosCampos_QuandoReducaoEstoqueSolicitacaoSerializada()
    {
        // Arrange
        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrdemServicoId = Guid.NewGuid(),
            Itens = new List<ItemReducao>
            {
                new ItemReducao { ItemEstoqueId = Guid.NewGuid(), Quantidade = 5 },
                new ItemReducao { ItemEstoqueId = Guid.NewGuid(), Quantidade = 3 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(solicitacao);
        var deserializado = JsonSerializer.Deserialize<ReducaoEstoqueSolicitacao>(json);

        // Assert
        deserializado.Should().NotBeNull();
        deserializado!.CorrelationId.Should().Be(solicitacao.CorrelationId);
        deserializado.OrdemServicoId.Should().Be(solicitacao.OrdemServicoId);
        deserializado.Itens.Should().HaveCount(2);
        deserializado.Itens[0].ItemEstoqueId.Should().Be(solicitacao.Itens[0].ItemEstoqueId);
        deserializado.Itens[0].Quantidade.Should().Be(solicitacao.Itens[0].Quantidade);
    }

    [Fact(DisplayName = "Deve ter valores corretos ao deserializar ReducaoEstoqueSolicitacao de JSON")]
    [Trait("Contrato", "ReducaoEstoque")]
    public void Deserializar_DeveTerValoresCorretos_QuandoReducaoEstoqueSolicitacaoDeserializadaDeJson()
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
        solicitacao.Should().NotBeNull();
        solicitacao!.CorrelationId.Should().Be(correlationId.ToString());
        solicitacao.OrdemServicoId.Should().Be(ordemServicoId);
        solicitacao.Itens.Should().HaveCount(2);
        solicitacao.Itens[0].ItemEstoqueId.Should().Be(itemId1);
        solicitacao.Itens[0].Quantidade.Should().Be(10);
        solicitacao.Itens[1].ItemEstoqueId.Should().Be(itemId2);
        solicitacao.Itens[1].Quantidade.Should().Be(5);
    }

    [Fact(DisplayName = "Deve conter todos os campos ao serializar ReducaoEstoqueResultado com sucesso")]
    [Trait("Contrato", "ReducaoEstoque")]
    public void Serializar_DeveConterTodosCampos_QuandoReducaoEstoqueResultadoComSucesso()
    {
        // Arrange
        var resultado = new ReducaoEstoqueResultado
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrdemServicoId = Guid.NewGuid(),
            Sucesso = true,
            MotivoFalha = null
        };

        // Act
        var json = JsonSerializer.Serialize(resultado);
        var deserializado = JsonSerializer.Deserialize<ReducaoEstoqueResultado>(json);

        // Assert
        deserializado.Should().NotBeNull();
        deserializado!.CorrelationId.Should().Be(resultado.CorrelationId);
        deserializado.OrdemServicoId.Should().Be(resultado.OrdemServicoId);
        deserializado.Sucesso.Should().BeTrue();
        deserializado.MotivoFalha.Should().BeNull();
    }

    [Fact(DisplayName = "Deve conter motivo de falha ao serializar ReducaoEstoqueResultado com falha")]
    [Trait("Contrato", "ReducaoEstoque")]
    public void Serializar_DeveConterMotivoFalha_QuandoReducaoEstoqueResultadoComFalha()
    {
        // Arrange
        var resultado = new ReducaoEstoqueResultado
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrdemServicoId = Guid.NewGuid(),
            Sucesso = false,
            MotivoFalha = "estoque_insuficiente"
        };

        // Act
        var json = JsonSerializer.Serialize(resultado);
        var deserializado = JsonSerializer.Deserialize<ReducaoEstoqueResultado>(json);

        // Assert
        deserializado.Should().NotBeNull();
        deserializado!.CorrelationId.Should().Be(resultado.CorrelationId);
        deserializado.OrdemServicoId.Should().Be(resultado.OrdemServicoId);
        deserializado.Sucesso.Should().BeFalse();
        deserializado.MotivoFalha.Should().Be("estoque_insuficiente");
    }

    [Fact(DisplayName = "Deve ter valores corretos ao deserializar ReducaoEstoqueResultado de JSON")]
    [Trait("Contrato", "ReducaoEstoque")]
    public void Deserializar_DeveTerValoresCorretos_QuandoReducaoEstoqueResultadoDeserializadoDeJson()
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
        resultado.Should().NotBeNull();
        resultado!.CorrelationId.Should().Be(correlationId.ToString());
        resultado.OrdemServicoId.Should().Be(ordemServicoId);
        resultado.Sucesso.Should().BeFalse();
        resultado.MotivoFalha.Should().Be("erro_interno");
    }

    [Fact(DisplayName = "Deve conter todos os campos ao serializar ItemReducao")]
    [Trait("Contrato", "ReducaoEstoque")]
    public void Serializar_DeveConterTodosCampos_QuandoItemReducaoSerializado()
    {
        // Arrange
        var item = new ItemReducao
        {
            ItemEstoqueId = Guid.NewGuid(),
            Quantidade = 7
        };

        // Act
        var json = JsonSerializer.Serialize(item);
        var deserializado = JsonSerializer.Deserialize<ItemReducao>(json);

        // Assert
        deserializado.Should().NotBeNull();
        deserializado!.ItemEstoqueId.Should().Be(item.ItemEstoqueId);
        deserializado.Quantidade.Should().Be(item.Quantidade);
    }

    [Theory(DisplayName = "Deve suportar diferentes motivos de falha no ReducaoEstoqueResultado")]
    [Trait("Contrato", "ReducaoEstoque")]
    [InlineData("estoque_insuficiente")]
    [InlineData("erro_interno")]
    [InlineData("servico_indisponivel")]
    public void Serializar_DeveSuportarDiferentesMotivosFalha(string motivoFalha)
    {
        // Arrange
        var resultado = new ReducaoEstoqueResultado
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrdemServicoId = Guid.NewGuid(),
            Sucesso = false,
            MotivoFalha = motivoFalha
        };

        // Act
        var json = JsonSerializer.Serialize(resultado);
        var deserializado = JsonSerializer.Deserialize<ReducaoEstoqueResultado>(json);

        // Assert
        deserializado.Should().NotBeNull();
        deserializado!.Sucesso.Should().BeFalse();
        deserializado.MotivoFalha.Should().Be(motivoFalha);
    }

    [Fact(DisplayName = "Deve serializar corretamente ReducaoEstoqueSolicitacao com itens vazios")]
    [Trait("Contrato", "ReducaoEstoque")]
    public void Serializar_DeveSerializarCorretamente_QuandoReducaoEstoqueSolicitacaoComItensVazios()
    {
        // Arrange
        var solicitacao = new ReducaoEstoqueSolicitacao
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrdemServicoId = Guid.NewGuid(),
            Itens = new List<ItemReducao>()
        };

        // Act
        var json = JsonSerializer.Serialize(solicitacao);
        var deserializado = JsonSerializer.Deserialize<ReducaoEstoqueSolicitacao>(json);

        // Assert
        deserializado.Should().NotBeNull();
        deserializado!.Itens.Should().NotBeNull();
        deserializado.Itens.Should().BeEmpty();
    }
}
