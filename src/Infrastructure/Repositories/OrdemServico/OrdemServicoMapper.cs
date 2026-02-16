using Domain.OrdemServico.Enums;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;
using ItemIncluidoEntity = Domain.OrdemServico.Aggregates.OrdemServico.ItemIncluido;
using ServicoIncluidoEntity = Domain.OrdemServico.Aggregates.OrdemServico.ServicoIncluido;
using OrcamentoEntity = Domain.OrdemServico.Aggregates.OrdemServico.Orcamento;

namespace Infrastructure.Repositories.OrdemServico
{
    /// <summary>
    /// Mapper entre Aggregate OrdemServico e OrdemServicoDocument (MongoDB)
    /// </summary>
    public static class OrdemServicoMapper
    {
        public static OrdemServicoDocument ToDocument(OrdemServicoAggregate aggregate)
        {
            return new OrdemServicoDocument
            {
                Id = aggregate.Id,
                VeiculoId = aggregate.VeiculoId,
                Codigo = aggregate.Codigo.Valor,
                Status = aggregate.Status.Valor.ToString(),
                Historico = new HistoricoTemporalDocument
                {
                    DataCriacao = aggregate.Historico.DataCriacao,
                    DataInicioExecucao = aggregate.Historico.DataInicioExecucao,
                    DataFinalizacao = aggregate.Historico.DataFinalizacao,
                    DataEntrega = aggregate.Historico.DataEntrega
                },
                InteracaoEstoque = new InteracaoEstoqueDocument
                {
                    DeveRemoverEstoque = aggregate.InteracaoEstoque.DeveRemoverEstoque,
                    EstoqueRemovidoComSucesso = aggregate.InteracaoEstoque.EstoqueRemovidoComSucesso
                },
                ServicosIncluidos = aggregate.ServicosIncluidos.Select(s => new ServicoIncluidoDocument
                {
                    Id = s.Id,
                    ServicoOriginalId = s.ServicoOriginalId,
                    Nome = s.Nome.Valor,
                    Preco = s.Preco.Valor
                }).ToList(),
                ItensIncluidos = aggregate.ItensIncluidos.Select(i => new ItemIncluidoDocument
                {
                    Id = i.Id,
                    ItemEstoqueOriginalId = i.ItemEstoqueOriginalId,
                    Nome = i.Nome.Valor,
                    Quantidade = i.Quantidade.Valor,
                    TipoItemIncluido = i.TipoItemIncluido.Valor.ToString(),
                    Preco = i.Preco.Valor
                }).ToList(),
                Orcamento = aggregate.Orcamento != null ? new OrcamentoDocument
                {
                    Id = aggregate.Orcamento.Id,
                    DataCriacao = aggregate.Orcamento.DataCriacao.Valor,
                    Preco = aggregate.Orcamento.Preco.Valor
                } : null
            };
        }

        public static OrdemServicoAggregate ToAggregate(OrdemServicoDocument document)
        {
            var statusEnum = Enum.Parse<StatusOrdemServicoEnum>(document.Status);

            var servicos = document.ServicosIncluidos
                .Select(s => ServicoIncluidoEntity.Reidratar(s.Id, s.ServicoOriginalId, s.Nome, s.Preco))
                .ToList();

            var itens = document.ItensIncluidos
                .Select(i => ItemIncluidoEntity.Reidratar(
                    i.Id,
                    i.ItemEstoqueOriginalId,
                    i.Nome,
                    i.Preco,
                    i.Quantidade,
                    Enum.Parse<TipoItemIncluidoEnum>(i.TipoItemIncluido)))
                .ToList();

            var orcamento = document.Orcamento != null
                ? OrcamentoEntity.Reidratar(document.Orcamento.Id, document.Orcamento.DataCriacao, document.Orcamento.Preco)
                : null;

            return OrdemServicoAggregate.Reidratar(
                document.Id,
                document.VeiculoId,
                document.Codigo,
                statusEnum,
                document.Historico.DataCriacao,
                document.Historico.DataInicioExecucao,
                document.Historico.DataFinalizacao,
                document.Historico.DataEntrega,
                document.InteracaoEstoque.DeveRemoverEstoque,
                document.InteracaoEstoque.EstoqueRemovidoComSucesso,
                servicos,
                itens,
                orcamento
            );
        }
    }
}
