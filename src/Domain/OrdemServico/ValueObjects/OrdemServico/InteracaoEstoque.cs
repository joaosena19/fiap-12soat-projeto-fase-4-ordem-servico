using Shared.Attributes;

namespace Domain.OrdemServico.ValueObjects.OrdemServico
{
    [ValueObject]
    public record InteracaoEstoque
    {
        public bool DeveRemoverEstoque { get; private init; }
        public bool? EstoqueRemovidoComSucesso { get; private init; }

        // Construtor privado para forçar uso dos factory methods
        private InteracaoEstoque() { }

        private InteracaoEstoque(bool deveRemoverEstoque, bool? estoqueRemovidoComSucesso)
        {
            DeveRemoverEstoque = deveRemoverEstoque;
            EstoqueRemovidoComSucesso = estoqueRemovidoComSucesso;
        }

        // Factory methods para criar os 3 estados possíveis

        /// <summary>
        /// Cria uma InteracaoEstoque indicando que a ordem NÃO precisa interagir com o Estoque.
        /// Usado quando a ordem não tem itens físicos.
        /// </summary>
        public static InteracaoEstoque SemInteracao() => new(false, null);

        /// <summary>
        /// Cria uma InteracaoEstoque indicando que a ordem PRECISA remover estoque e está aguardando confirmação.
        /// Usado quando a ordem tem itens e a saga foi iniciada.
        /// </summary>
        public static InteracaoEstoque AguardandoReducao() => new(true, null);

        // Métodos de transição de estado

        /// <summary>
        /// Marca a redução de estoque como confirmada (sucesso).
        /// Usado quando o Estoque responde com sucesso via mensageria.
        /// </summary>
        public InteracaoEstoque ConfirmarReducao() => this with { EstoqueRemovidoComSucesso = true };

        /// <summary>
        /// Marca a redução de estoque como falhada ou compensada.
        /// Usado quando o Estoque responde com falha, ou quando o BackgroundService detecta timeout.
        /// </summary>
        public InteracaoEstoque MarcarFalha() => this with { EstoqueRemovidoComSucesso = false };

        // Propriedades computadas para facilitar decisões

        /// <summary>
        /// Indica se a ordem está aguardando resposta do Estoque.
        /// Verdadeiro quando DeveRemoverEstoque == true E EstoqueRemovidoComSucesso == null.
        /// </summary>
        public bool EstaAguardandoEstoque => DeveRemoverEstoque && EstoqueRemovidoComSucesso == null;

        /// <summary>
        /// Indica se o estoque foi confirmado (ou se não era necessário).
        /// Verdadeiro quando: (a) NÃO precisa remover estoque, OU (b) redução foi confirmada com sucesso.
        /// </summary>
        public bool EstoqueFoiConfirmado => !DeveRemoverEstoque || EstoqueRemovidoComSucesso == true;
    }
}
