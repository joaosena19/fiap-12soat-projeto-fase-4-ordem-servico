using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;
using Domain.OrdemServico.Enums;
using FluentAssertions;
using Shared.Exceptions;
using TipoItemIncluidoEnum = Domain.OrdemServico.Enums.TipoItemIncluidoEnum;

namespace Tests.Domain.OrdemServico
{
    public class OrdemServicoSagaUnitTest
    {
        #region Helper Methods

        private OrdemServicoAggregate CriarOrdemServicoComOrcamento(bool comItens = true)
        {
            var os = OrdemServicoAggregate.Criar(Guid.NewGuid());
            os.AlterarStatus(StatusOrdemServicoEnum.EmDiagnostico);

            // Adiciona serviço
            os.AdicionarServico(Guid.NewGuid(), "Troca de óleo", 150.00m);

            if (comItens)
            {
                // Adiciona item de estoque
                os.AdicionarItem(Guid.NewGuid(), "Filtro de óleo", 50.00m, 2, TipoItemIncluidoEnum.Peca);
            }

            os.GerarOrcamento();
            return os;
        }

        #endregion

        #region Testes AprovarOrcamento - Novo comportamento

        [Fact(DisplayName = "AprovarOrcamento quando AguardandoAprovacao deve mudar para Aprovada")]
        [Trait("Método", "AprovarOrcamento")]
        public void AprovarOrcamento_QuandoAguardandoAprovacao_MudaParaAprovada()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();

            // Act
            os.AprovarOrcamento();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);
        }

        [Fact(DisplayName = "AprovarOrcamento não deve mudar diretamente para EmExecucao")]
        [Trait("Método", "AprovarOrcamento")]
        public void AprovarOrcamento_NaoDeveMudarDiretamenteParaEmExecucao()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();

            // Act
            os.AprovarOrcamento();

            // Assert
            os.Status.Valor.Should().NotBe(StatusOrdemServicoEnum.EmExecucao);
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);
        }

        [Fact(DisplayName = "AprovarOrcamento quando não é AguardandoAprovacao deve lançar exceção")]
        [Trait("Método", "AprovarOrcamento")]
        public void AprovarOrcamento_QuandoNaoEstaAguardandoAprovacao_LancaExcecao()
        {
            // Arrange
            var os = OrdemServicoAggregate.Criar(Guid.NewGuid());

            // Act & Assert
            FluentActions.Invoking(() => os.AprovarOrcamento())
                .Should().Throw<DomainException>()
                .WithMessage("*aprovar orçamento com status*");
        }

        #endregion

        #region Testes IniciarExecucao - Novo comportamento

        [Fact(DisplayName = "IniciarExecucao quando Aprovada deve mudar para EmExecucao")]
        [Trait("Método", "IniciarExecucao")]
        public void IniciarExecucao_QuandoAprovada_MudaParaEmExecucao()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();

            // Act
            os.IniciarExecucao();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
        }

        [Fact(DisplayName = "IniciarExecucao quando tem itens deve setar InteracaoEstoque como AguardandoReducao")]
        [Trait("Método", "IniciarExecucao")]
        public void IniciarExecucao_QuandoTemItens_SetaInteracaoEstoqueAguardando()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento(comItens: true);
            os.AprovarOrcamento();

            // Act
            os.IniciarExecucao();

            // Assert
            os.InteracaoEstoque.DeveRemoverEstoque.Should().BeTrue();
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeNull();
            os.InteracaoEstoque.EstaAguardandoEstoque.Should().BeTrue();
        }

        [Fact(DisplayName = "IniciarExecucao quando não tem itens deve setar InteracaoEstoque como SemInteracao")]
        [Trait("Método", "IniciarExecucao")]
        public void IniciarExecucao_QuandoSemItens_SetaInteracaoEstoqueSemInteracao()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento(comItens: false);
            os.AprovarOrcamento();

            // Act
            os.IniciarExecucao();

            // Assert
            os.InteracaoEstoque.DeveRemoverEstoque.Should().BeFalse();
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeNull();
            os.InteracaoEstoque.EstaAguardandoEstoque.Should().BeFalse();
            os.InteracaoEstoque.EstoqueFoiConfirmado.Should().BeTrue();
        }

        [Fact(DisplayName = "IniciarExecucao quando não está Aprovada deve lançar exceção")]
        [Trait("Método", "IniciarExecucao")]
        public void IniciarExecucao_QuandoNaoAprovada_LancaExcecao()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();

            // Act & Assert
            FluentActions.Invoking(() => os.IniciarExecucao())
                .Should().Throw<DomainException>()
                .WithMessage("*iniciar execução*Aprovada*");
        }

        #endregion

        #region Testes Cancelar - Bloqueio após Aprovada

        [Fact(DisplayName = "Cancelar quando Aprovada deve lançar exceção")]
        [Trait("Método", "Cancelar")]
        public void Cancelar_QuandoAprovada_LancaDomainException()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();

            // Act & Assert
            FluentActions.Invoking(() => os.Cancelar())
                .Should().Throw<DomainException>()
                .WithMessage("*cancelar ordem de serviço com status Aprovada*");
        }

        [Fact(DisplayName = "Cancelar quando EmExecucao deve lançar exceção")]
        [Trait("Método", "Cancelar")]
        public void Cancelar_QuandoEmExecucao_LancaDomainException()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento(comItens: false);
            os.AprovarOrcamento();
            os.IniciarExecucao();

            // Act & Assert
            FluentActions.Invoking(() => os.Cancelar())
                .Should().Throw<DomainException>()
                .WithMessage("*cancelar ordem de serviço com status EmExecucao*");
        }

        [Fact(DisplayName = "Cancelar quando Finalizada deve lançar exceção")]
        [Trait("Método", "Cancelar")]
        public void Cancelar_QuandoFinalizada_LancaDomainException()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento(comItens: false);
            os.AprovarOrcamento();
            os.IniciarExecucao();
            os.FinalizarExecucao();

            // Act & Assert
            FluentActions.Invoking(() => os.Cancelar())
                .Should().Throw<DomainException>()
                .WithMessage("*cancelar ordem de serviço com status Finalizada*");
        }

        [Fact(DisplayName = "Cancelar quando Entregue deve lançar exceção")]
        [Trait("Método", "Cancelar")]
        public void Cancelar_QuandoEntregue_LancaDomainException()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento(comItens: false);
            os.AprovarOrcamento();
            os.IniciarExecucao();
            os.FinalizarExecucao();
            os.Entregar();

            // Act & Assert
            FluentActions.Invoking(() => os.Cancelar())
                .Should().Throw<DomainException>()
                .WithMessage("*cancelar ordem de serviço com status Entregue*");
        }

        [Fact(DisplayName = "Cancelar quando AguardandoAprovacao deve funcionar normalmente")]
        [Trait("Método", "Cancelar")]
        public void Cancelar_QuandoAguardandoAprovacao_FuncionaNormalmente()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();

            // Act
            os.Cancelar();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
        }

        [Fact(DisplayName = "Cancelar quando Recebida deve funcionar normalmente")]
        [Trait("Método", "Cancelar")]
        public void Cancelar_QuandoRecebida_FuncionaNormalmente()
        {
            // Arrange
            var os = OrdemServicoAggregate.Criar(Guid.NewGuid());

            // Act
            os.Cancelar();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
        }

        #endregion

        #region Testes ConfirmarReducaoEstoque

        [Fact(DisplayName = "ConfirmarReducaoEstoque deve setar EstoqueRemovidoComSucesso como true")]
        [Trait("Método", "ConfirmarReducaoEstoque")]
        public void ConfirmarReducaoEstoque_SetaEstoqueRemovidoComSucessoTrue()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();
            os.IniciarExecucao();

            // Act
            os.ConfirmarReducaoEstoque();

            // Assert
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeTrue();
            os.InteracaoEstoque.EstaAguardandoEstoque.Should().BeFalse();
            os.InteracaoEstoque.EstoqueFoiConfirmado.Should().BeTrue();
        }

        [Fact(DisplayName = "ConfirmarReducaoEstoque independe do status atual da OS")]
        [Trait("Método", "ConfirmarReducaoEstoque")]
        public void ConfirmarReducaoEstoque_IndependeDoStatusAtual()
        {
            // Arrange - OS compensada, voltou para Aprovada
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();
            os.IniciarExecucao();
            os.CompensarFalhaSaga(); // Volta para Aprovada

            // Act - Estoque responde com sucesso (race condition)
            os.ConfirmarReducaoEstoque();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeTrue();
        }

        #endregion

        #region Testes CompensarFalhaSaga

        [Fact(DisplayName = "CompensarFalhaSaga quando EmExecucao deve voltar para Aprovada")]
        [Trait("Método", "CompensarFalhaSaga")]
        public void CompensarFalhaSaga_QuandoEmExecucao_VoltaParaAprovada()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();
            os.IniciarExecucao();

            // Act
            os.CompensarFalhaSaga();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeFalse();
            os.InteracaoEstoque.EstoqueFoiConfirmado.Should().BeFalse();
        }

        [Fact(DisplayName = "CompensarFalhaSaga quando já Aprovada não deve alterar status")]
        [Trait("Método", "CompensarFalhaSaga")]
        public void CompensarFalhaSaga_QuandoJaAprovada_NaoAlteraStatus()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();

            // Act
            os.CompensarFalhaSaga();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeFalse();
        }

        [Fact(DisplayName = "CompensarFalhaSaga quando Recebida não altera status")]
        [Trait("Método", "CompensarFalhaSaga")]
        public void CompensarFalhaSaga_QuandoRecebida_NaoAlteraStatus()
        {
            // Arrange
            var os = OrdemServicoAggregate.Criar(Guid.NewGuid());

            // Act
            os.CompensarFalhaSaga();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Recebida);
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeFalse();
        }

        #endregion

        #region Testes FinalizarExecucao - Guard estoque não confirmado

        [Fact(DisplayName = "FinalizarExecucao quando estoque não confirmado deve lançar exceção")]
        [Trait("Método", "FinalizarExecucao")]
        public void FinalizarExecucao_QuandoEstoqueNaoConfirmado_LancaDomainException()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();
            os.IniciarExecucao();

            // Act & Assert
            FluentActions.Invoking(() => os.FinalizarExecucao())
                .Should().Throw<DomainException>()
                .WithMessage("*finalizar execução enquanto aguardando confirmação do estoque*");
        }

        [Fact(DisplayName = "FinalizarExecucao quando estoque confirmado deve mudar para Finalizada")]
        [Trait("Método", "FinalizarExecucao")]
        public void FinalizarExecucao_QuandoEstoqueConfirmado_MudaParaFinalizada()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();
            os.AprovarOrcamento();
            os.IniciarExecucao();
            os.ConfirmarReducaoEstoque();

            // Act
            os.FinalizarExecucao();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Finalizada);
        }

        [Fact(DisplayName = "FinalizarExecucao quando sem interação com estoque deve mudar para Finalizada")]
        [Trait("Método", "FinalizarExecucao")]
        public void FinalizarExecucao_QuandoSemInteracao_MudaParaFinalizada()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento(comItens: false);
            os.AprovarOrcamento();
            os.IniciarExecucao();

            // Act
            os.FinalizarExecucao();

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Finalizada);
            os.InteracaoEstoque.EstoqueFoiConfirmado.Should().BeTrue();
        }

        #endregion

        #region Testes AlterarStatus - Status Aprovada

        [Fact(DisplayName = "AlterarStatus deve chamar AprovarOrcamento quando status for Aprovada")]
        [Trait("Método", "AlterarStatus")]
        public void AlterarStatus_ComAprovada_DeveChamarAprovarOrcamento()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();

            // Act
            os.AlterarStatus(StatusOrdemServicoEnum.Aprovada);

            // Assert
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);
        }

        #endregion

        #region Testes Cenários Completos da Saga

        [Fact(DisplayName = "Saga completa com sucesso - caminho feliz")]
        [Trait("Cenário", "Saga")]
        public void SagaCompleta_CaminhoFeliz_DeveCompletarComSucesso()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();

            // Act & Assert - Fluxo completo
            os.AprovarOrcamento();
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);

            os.IniciarExecucao();
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
            os.InteracaoEstoque.EstaAguardandoEstoque.Should().BeTrue();

            os.ConfirmarReducaoEstoque();
            os.InteracaoEstoque.EstoqueFoiConfirmado.Should().BeTrue();

            os.FinalizarExecucao();
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Finalizada);

            os.Entregar();
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Entregue);
        }

        [Fact(DisplayName = "Saga com compensação e retry com sucesso")]
        [Trait("Cenário", "Saga")]
        public void SagaComCompensacao_RetryComSucesso()
        {
            // Arrange
            var os = CriarOrdemServicoComOrcamento();

            // Act & Assert - Primeira tentativa
            os.AprovarOrcamento();
            os.IniciarExecucao();

            // Estoque falha ou timeout
            os.CompensarFalhaSaga();
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Aprovada);
            os.InteracaoEstoque.EstoqueRemovidoComSucesso.Should().BeFalse();

            // Re-tentativa
            os.IniciarExecucao();
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
            os.InteracaoEstoque.EstaAguardandoEstoque.Should().BeTrue();

            // Estoque confirma
            os.ConfirmarReducaoEstoque();
            os.FinalizarExecucao();
            os.Status.Valor.Should().Be(StatusOrdemServicoEnum.Finalizada);
        }

        #endregion
    }
}
