using API.Presenters.OrdemServico;
using Application.OrdemServico.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.Application.SharedHelpers.AggregateBuilders;
using Xunit;

namespace Tests.API.Presenters.OrdemServico
{
    public class BuscarOrdensServicoPresenterTests
    {
        [Fact(DisplayName = "Deve mapear lista de ordens de serviço quando existirem ordens")]
        [Trait("Presenter", "BuscarOrdensServico")]
        public void ApresentarSucesso_DeveMapearListaDeOrdensServico_QuandoExistiremOrdens()
        {
            // Arrange
            var servicoSemOrcamentoId = Guid.NewGuid();
            var servicoSemOrcamentoNome = "Troca de óleo";
            var servicoSemOrcamentoPreco = 150.00m;
            var servicoSemOrcamentoData = new ServicoData(servicoSemOrcamentoId, servicoSemOrcamentoNome, servicoSemOrcamentoPreco);

            var itemSemOrcamentoId = Guid.NewGuid();
            var itemSemOrcamentoNome = "Filtro de óleo";
            var itemSemOrcamentoPreco = 35.00m;
            var itemSemOrcamentoQuantidade = 2;
            var itemSemOrcamentoTipo = global::Domain.OrdemServico.Enums.TipoItemIncluidoEnum.Peca;
            var itemSemOrcamentoData = new ItemData(itemSemOrcamentoId, itemSemOrcamentoNome, itemSemOrcamentoPreco, itemSemOrcamentoQuantidade, itemSemOrcamentoTipo);

            var ordemSemOrcamento = new OrdemServicoBuilder()
                .ComServicos(servicoSemOrcamentoData)
                .ComItens(itemSemOrcamentoData)
                .Build();

            var servicoComOrcamentoId = Guid.NewGuid();
            var servicoComOrcamentoNome = "Alinhamento e balanceamento";
            var servicoComOrcamentoPreco = 200.00m;
            var servicoComOrcamentoData = new ServicoData(servicoComOrcamentoId, servicoComOrcamentoNome, servicoComOrcamentoPreco);

            var itemComOrcamentoId = Guid.NewGuid();
            var itemComOrcamentoNome = "Pastilha de freio";
            var itemComOrcamentoPreco = 120.00m;
            var itemComOrcamentoQuantidade = 4;
            var itemComOrcamentoTipo = global::Domain.OrdemServico.Enums.TipoItemIncluidoEnum.Peca;
            var itemComOrcamentoData = new ItemData(itemComOrcamentoId, itemComOrcamentoNome, itemComOrcamentoPreco, itemComOrcamentoQuantidade, itemComOrcamentoTipo);

            var ordemComOrcamento = new OrdemServicoBuilder()
                .ComServicos(servicoComOrcamentoData)
                .ComItens(itemComOrcamentoData)
                .ComOrcamento(comItens: false)
                .Build();

            var presenter = new BuscarOrdensServicoPresenter();

            // Act
            presenter.ApresentarSucesso(new[] { ordemSemOrcamento, ordemComOrcamento });

            // Assert
            presenter.FoiSucesso.Should().BeTrue();
            
            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<OkObjectResult>();
            
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeAssignableTo<IEnumerable<RetornoOrdemServicoCompletaDto>>();
            
            var retorno = okResult.Value as IEnumerable<RetornoOrdemServicoCompletaDto>;
            var lista = retorno!.ToList();
            
            lista.Should().HaveCount(2);

            var primeiraOrdem = lista[0];
            primeiraOrdem.Id.Should().Be(ordemSemOrcamento.Id);
            primeiraOrdem.Codigo.Should().Be(ordemSemOrcamento.Codigo.Valor);
            primeiraOrdem.VeiculoId.Should().Be(ordemSemOrcamento.VeiculoId);
            primeiraOrdem.Status.Should().Be(ordemSemOrcamento.Status.Valor.ToString());
            primeiraOrdem.DataCriacao.Should().Be(ordemSemOrcamento.Historico.DataCriacao);

            primeiraOrdem.ServicosIncluidos.Should().HaveCount(1);
            var servicoSemOrcamentoMapeado = primeiraOrdem.ServicosIncluidos[0];
            servicoSemOrcamentoMapeado.ServicoOriginalId.Should().Be(servicoSemOrcamentoId);
            servicoSemOrcamentoMapeado.Nome.Should().Be(servicoSemOrcamentoNome);
            servicoSemOrcamentoMapeado.Preco.Should().Be(servicoSemOrcamentoPreco);

            primeiraOrdem.ItensIncluidos.Should().HaveCount(1);
            var itemSemOrcamentoMapeado = primeiraOrdem.ItensIncluidos[0];
            itemSemOrcamentoMapeado.ItemEstoqueOriginalId.Should().Be(itemSemOrcamentoId);
            itemSemOrcamentoMapeado.Nome.Should().Be(itemSemOrcamentoNome);
            itemSemOrcamentoMapeado.Preco.Should().Be(itemSemOrcamentoPreco);
            itemSemOrcamentoMapeado.Quantidade.Should().Be(itemSemOrcamentoQuantidade);
            itemSemOrcamentoMapeado.TipoItemIncluido.Should().Be(itemSemOrcamentoTipo.ToString());

            primeiraOrdem.Orcamento.Should().BeNull();

            var segundaOrdem = lista[1];
            segundaOrdem.Id.Should().Be(ordemComOrcamento.Id);
            segundaOrdem.Codigo.Should().Be(ordemComOrcamento.Codigo.Valor);
            segundaOrdem.VeiculoId.Should().Be(ordemComOrcamento.VeiculoId);
            segundaOrdem.Status.Should().Be(ordemComOrcamento.Status.Valor.ToString());
            segundaOrdem.DataCriacao.Should().Be(ordemComOrcamento.Historico.DataCriacao);

            segundaOrdem.ServicosIncluidos.Should().HaveCount(1);
            var servicoComOrcamentoMapeado = segundaOrdem.ServicosIncluidos[0];
            servicoComOrcamentoMapeado.ServicoOriginalId.Should().Be(servicoComOrcamentoId);
            servicoComOrcamentoMapeado.Nome.Should().Be(servicoComOrcamentoNome);
            servicoComOrcamentoMapeado.Preco.Should().Be(servicoComOrcamentoPreco);

            segundaOrdem.ItensIncluidos.Should().HaveCount(1);
            var itemComOrcamentoMapeado = segundaOrdem.ItensIncluidos[0];
            itemComOrcamentoMapeado.ItemEstoqueOriginalId.Should().Be(itemComOrcamentoId);
            itemComOrcamentoMapeado.Nome.Should().Be(itemComOrcamentoNome);
            itemComOrcamentoMapeado.Preco.Should().Be(itemComOrcamentoPreco);
            itemComOrcamentoMapeado.Quantidade.Should().Be(itemComOrcamentoQuantidade);
            itemComOrcamentoMapeado.TipoItemIncluido.Should().Be(itemComOrcamentoTipo.ToString());

            segundaOrdem.Orcamento.Should().NotBeNull();
            segundaOrdem.Orcamento!.Id.Should().Be(ordemComOrcamento.Orcamento!.Id);
            segundaOrdem.Orcamento.Preco.Should().Be(ordemComOrcamento.Orcamento.Preco.Valor);
            segundaOrdem.Orcamento.DataCriacao.Should().Be(ordemComOrcamento.Orcamento.DataCriacao.Valor);
        }
    }
}
