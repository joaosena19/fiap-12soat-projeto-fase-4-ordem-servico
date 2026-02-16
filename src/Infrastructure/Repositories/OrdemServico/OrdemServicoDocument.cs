using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Repositories.OrdemServico
{
    /// <summary>
    /// Documento MongoDB para OrdemServico - Modelo de persistência
    /// </summary>
    public class OrdemServicoDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        
        public Guid VeiculoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public HistoricoTemporalDocument Historico { get; set; } = null!;
        public InteracaoEstoqueDocument InteracaoEstoque { get; set; } = null!;
        public List<ServicoIncluidoDocument> ServicosIncluidos { get; set; } = new();
        public List<ItemIncluidoDocument> ItensIncluidos { get; set; } = new();
        public OrcamentoDocument? Orcamento { get; set; }
    }

    public class HistoricoTemporalDocument
    {
        public DateTime DataCriacao { get; set; }
        public DateTime? DataInicioExecucao { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public DateTime? DataEntrega { get; set; }
    }

    public class InteracaoEstoqueDocument
    {
        public bool DeveRemoverEstoque { get; set; }
        public bool? EstoqueRemovidoComSucesso { get; set; }
    }

    public class ServicoIncluidoDocument
    {
        public Guid Id { get; set; }
        public Guid ServicoOriginalId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }

    public class ItemIncluidoDocument
    {
        public Guid Id { get; set; }
        public Guid ItemEstoqueOriginalId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string TipoItemIncluido { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }

    public class OrcamentoDocument
    {
        public Guid Id { get; set; }
        public DateTime DataCriacao { get; set; }
        public decimal Preco { get; set; }
    }
}
