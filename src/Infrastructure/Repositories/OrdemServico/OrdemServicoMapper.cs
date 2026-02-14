using Domain.OrdemServico.Enums;
using Domain.OrdemServico.ValueObjects.ItemIncluido;
using Domain.OrdemServico.ValueObjects.Orcamento;
using Domain.OrdemServico.ValueObjects.OrdemServico;
using Domain.OrdemServico.ValueObjects.ServicoIncluido;
using System.Reflection;
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
            // Criar instância via reflection (construtor privado)
            var aggregate = (OrdemServicoAggregate)Activator.CreateInstance(typeof(OrdemServicoAggregate), true)!;

            // Definir Id (private init)
            SetProperty(aggregate, "Id", document.Id);

            // Definir VeiculoId
            SetProperty(aggregate, "VeiculoId", document.VeiculoId);

            // Definir Codigo
            SetProperty(aggregate, "Codigo", new Codigo(document.Codigo));

            // Definir Status
            var statusEnum = Enum.Parse<StatusOrdemServicoEnum>(document.Status);
            SetProperty(aggregate, "Status", new Status(statusEnum));

            // Definir Historico
            var historico = new HistoricoTemporal(
                document.Historico.DataCriacao,
                document.Historico.DataInicioExecucao,
                document.Historico.DataFinalizacao,
                document.Historico.DataEntrega
            );
            SetProperty(aggregate, "Historico", historico);

            // Definir InteracaoEstoque
            var interacaoEstoque = document.InteracaoEstoque.DeveRemoverEstoque
                ? (document.InteracaoEstoque.EstoqueRemovidoComSucesso.HasValue
                    ? (document.InteracaoEstoque.EstoqueRemovidoComSucesso.Value
                        ? InteracaoEstoque.AguardandoReducao().ConfirmarReducao()
                        : InteracaoEstoque.AguardandoReducao().MarcarFalha())
                    : InteracaoEstoque.AguardandoReducao())
                : InteracaoEstoque.SemInteracao();
            SetProperty(aggregate, "InteracaoEstoque", interacaoEstoque);

            // Definir ServicosIncluidos (lista privada)
            var servicosField = typeof(OrdemServicoAggregate).GetField("_servicosIncluidos", BindingFlags.NonPublic | BindingFlags.Instance);
            var servicos = document.ServicosIncluidos.Select(s => CreateServicoIncluido(s)).ToList();
            servicosField!.SetValue(aggregate, servicos);

            // Definir ItensIncluidos (lista privada)
            var itensField = typeof(OrdemServicoAggregate).GetField("_itensIncluidos", BindingFlags.NonPublic | BindingFlags.Instance);
            var itens = document.ItensIncluidos.Select(i => CreateItemIncluido(i)).ToList();
            itensField!.SetValue(aggregate, itens);

            // Definir Orcamento
            if (document.Orcamento != null)
            {
                var orcamento = CreateOrcamento(document.Orcamento);
                SetProperty(aggregate, "Orcamento", orcamento);
            }

            return aggregate;
        }

        private static ServicoIncluidoEntity CreateServicoIncluido(ServicoIncluidoDocument doc)
        {
            var servico = (ServicoIncluidoEntity)Activator.CreateInstance(typeof(ServicoIncluidoEntity), true)!;
            SetProperty(servico, "Id", doc.Id);
            SetProperty(servico, "ServicoOriginalId", doc.ServicoOriginalId);
            SetProperty(servico, "Nome", new NomeServico(doc.Nome));
            SetProperty(servico, "Preco", new PrecoServico(doc.Preco));
            return servico;
        }

        private static ItemIncluidoEntity CreateItemIncluido(ItemIncluidoDocument doc)
        {
            var item = (ItemIncluidoEntity)Activator.CreateInstance(typeof(ItemIncluidoEntity), true)!;
            SetProperty(item, "Id", doc.Id);
            SetProperty(item, "ItemEstoqueOriginalId", doc.ItemEstoqueOriginalId);
            SetProperty(item, "Nome", new Nome(doc.Nome));
            SetProperty(item, "Quantidade", new Quantidade(doc.Quantidade));
            var tipoEnum = Enum.Parse<TipoItemIncluidoEnum>(doc.TipoItemIncluido);
            SetProperty(item, "TipoItemIncluido", new TipoItemIncluido(tipoEnum));
            SetProperty(item, "Preco", new PrecoItem(doc.Preco));
            return item;
        }

        private static OrcamentoEntity CreateOrcamento(OrcamentoDocument doc)
        {
            var orcamento = (OrcamentoEntity)Activator.CreateInstance(typeof(OrcamentoEntity), true)!;
            SetProperty(orcamento, "Id", doc.Id);
            SetProperty(orcamento, "DataCriacao", new DataCriacao(doc.DataCriacao));
            SetProperty(orcamento, "Preco", new PrecoOrcamento(doc.Preco));
            return orcamento;
        }

        private static void SetProperty<T>(object obj, string propertyName, T value)
        {
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
            else
            {
                // Tentar setar via backing field para propriedades init
                var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                field?.SetValue(obj, value);
            }
        }
    }
}
