using Domain.OrdemServico.ValueObjects.OrdemServico;
using FluentAssertions;

namespace Tests.Domain.OrdemServico
{
    public class InteracaoEstoqueUnitTest
    {
        #region Testes Factory Methods

        [Fact(DisplayName = "SemInteracao deve criar InteracaoEstoque com DeveRemoverEstoque = false")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void SemInteracao_DeveRemoverEstoque_RetornaFalse()
        {
            // Act
            var interacao = InteracaoEstoque.SemInteracao();

            // Assert
            interacao.DeveRemoverEstoque.Should().BeFalse();
            interacao.EstoqueRemovidoComSucesso.Should().BeNull();
            interacao.EstaAguardandoRemocaoEstoque.Should().BeFalse();
            interacao.SemPendenciasEstoque.Should().BeTrue(); // Sem interação = já confirmado
        }

        [Fact(DisplayName = "AguardandoReducao deve criar InteracaoEstoque aguardando resposta do Estoque")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void AguardandoReducao_EstaAguardandoEstoque_RetornaTrue()
        {
            // Act
            var interacao = InteracaoEstoque.AguardandoReducao();

            // Assert
            interacao.DeveRemoverEstoque.Should().BeTrue();
            interacao.EstoqueRemovidoComSucesso.Should().BeNull();
            interacao.EstaAguardandoRemocaoEstoque.Should().BeTrue();
            interacao.SemPendenciasEstoque.Should().BeFalse(); // Ainda aguardando
        }

        #endregion

        #region Testes Métodos de Transição

        [Fact(DisplayName = "ConfirmarReducao deve marcar EstoqueRemovidoComSucesso como true")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void ConfirmarReducao_EstoqueFoiConfirmado_RetornaTrue()
        {
            // Arrange
            var interacao = InteracaoEstoque.AguardandoReducao();

            // Act
            var interacaoConfirmada = interacao.ConfirmarReducao();

            // Assert
            interacaoConfirmada.DeveRemoverEstoque.Should().BeTrue();
            interacaoConfirmada.EstoqueRemovidoComSucesso.Should().BeTrue();
            interacaoConfirmada.EstaAguardandoRemocaoEstoque.Should().BeFalse();
            interacaoConfirmada.SemPendenciasEstoque.Should().BeTrue();
        }

        [Fact(DisplayName = "MarcarFalha deve marcar EstoqueRemovidoComSucesso como false")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void MarcarFalha_EstoqueRemovidoComSucesso_RetornaFalse()
        {
            // Arrange
            var interacao = InteracaoEstoque.AguardandoReducao();

            // Act
            var interacaoFalhada = interacao.MarcarFalha();

            // Assert
            interacaoFalhada.DeveRemoverEstoque.Should().BeTrue();
            interacaoFalhada.EstoqueRemovidoComSucesso.Should().BeFalse();
            interacaoFalhada.EstaAguardandoRemocaoEstoque.Should().BeFalse();
            interacaoFalhada.SemPendenciasEstoque.Should().BeFalse();
        }

        #endregion

        #region Testes Propriedades Computadas

        [Fact(DisplayName = "EstaAguardandoRemocaoEstoque deve retornar true apenas quando aguardando")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void EstaAguardandoEstoque_ApenasSemInteracao_RetornaTrue()
        {
            // Arrange & Act
            var semInteracao = InteracaoEstoque.SemInteracao();
            var aguardando = InteracaoEstoque.AguardandoReducao();
            var confirmado = aguardando.ConfirmarReducao();
            var falhado = aguardando.MarcarFalha();

            // Assert
            semInteracao.EstaAguardandoRemocaoEstoque.Should().BeFalse("não precisa remover estoque");
            aguardando.EstaAguardandoRemocaoEstoque.Should().BeTrue("está aguardando resposta");
            confirmado.EstaAguardandoRemocaoEstoque.Should().BeFalse("já foi confirmado");
            falhado.EstaAguardandoRemocaoEstoque.Should().BeFalse("já foi marcado como falha");
        }

        [Fact(DisplayName = "SemPendenciasEstoque deve retornar true quando não precisa remover ou quando confirmado")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void EstoqueFoiConfirmado_SemInteracaoOuConfirmado_RetornaTrue()
        {
            // Arrange & Act
            var semInteracao = InteracaoEstoque.SemInteracao();
            var aguardando = InteracaoEstoque.AguardandoReducao();
            var confirmado = aguardando.ConfirmarReducao();
            var falhado = aguardando.MarcarFalha();

            // Assert
            semInteracao.SemPendenciasEstoque.Should().BeTrue("não precisa remover estoque");
            aguardando.SemPendenciasEstoque.Should().BeFalse("ainda aguardando");
            confirmado.SemPendenciasEstoque.Should().BeTrue("foi confirmado com sucesso");
            falhado.SemPendenciasEstoque.Should().BeFalse("falhou ou foi compensado");
        }

        #endregion

        #region Testes Imutabilidade (Record)

        [Fact(DisplayName = "ConfirmarReducao deve retornar nova instância sem alterar a original")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void ConfirmarReducao_DeveSerImutavel()
        {
            // Arrange
            var original = InteracaoEstoque.AguardandoReducao();

            // Act
            var modificado = original.ConfirmarReducao();

            // Assert
            original.EstoqueRemovidoComSucesso.Should().BeNull("original não deve ser alterado");
            modificado.EstoqueRemovidoComSucesso.Should().BeTrue("nova instância deve ter confirmação");
        }

        [Fact(DisplayName = "MarcarFalha deve retornar nova instância sem alterar a original")]
        [Trait("ValueObject", "InteracaoEstoque")]
        public void MarcarFalha_DeveSerImutavel()
        {
            // Arrange
            var original = InteracaoEstoque.AguardandoReducao();

            // Act
            var modificado = original.MarcarFalha();

            // Assert
            original.EstoqueRemovidoComSucesso.Should().BeNull("original não deve ser alterado");
            modificado.EstoqueRemovidoComSucesso.Should().BeFalse("nova instância deve ter falha marcada");
        }

        #endregion
    }
}
