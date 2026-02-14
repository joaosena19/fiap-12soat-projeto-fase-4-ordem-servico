using Domain.OrdemServico.ValueObjects.ServicoIncluido;
using Shared.Attributes;
using UUIDNext;

namespace Domain.OrdemServico.Aggregates.OrdemServico
{
    [AggregateMember]
    public class ServicoIncluido
    {
        public Guid Id { get; private init; }
        public Guid ServicoOriginalId { get; private set; }
        public NomeServico Nome { get; private set; } = null!;
        public PrecoServico Preco { get; private set; } = null!;

        // Construtor sem parâmetro para EF Core
        private ServicoIncluido() { }

        private ServicoIncluido(Guid id, Guid servicoOriginalId, NomeServico nome, PrecoServico preco)
        {
            Id = id;
            ServicoOriginalId = servicoOriginalId;
            Nome = nome;
            Preco = preco;
        }

        public static ServicoIncluido Criar(Guid servicoOriginalId, string nome, decimal preco)
        {
            return new ServicoIncluido(Uuid.NewSequential(), servicoOriginalId, new NomeServico(nome), new PrecoServico(preco));
        }

        /// <summary>
        /// Reidrata o ServicoIncluido a partir de dados do banco.
        /// NÃO deve ser usado fora do contexto de buscar do banco.
        /// </summary>
        public static ServicoIncluido Reidratar(Guid id, Guid servicoOriginalId, string nome, decimal preco)
        {
            return new ServicoIncluido(id, servicoOriginalId, new NomeServico(nome), new PrecoServico(preco));
        }
    }

}
