using API.Presenters.OrdemServico;
using Application.OrdemServico.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.Application.SharedHelpers.AggregateBuilders;
using Xunit;

namespace Tests.API.Presenters.OrdemServico
{
    public class AdicionarItemPresenterTests
    {
        [Fact(DisplayName = "Deve mapear DTO com serviços e itens quando ordem de serviço válida")]
        [Trait("Presenter", "AdicionarItemPresenter")]
        public void ApresentarSucesso_DeveMapearDto_QuandoOrdemServicoValida()
        {
            // Arrange
            var servicoId = Guid.NewGuid();
            var servicoNome = "Troca de óleo";
            var servicoPreco = 150.00m;
            var servicoData = new ServicoData(servicoId, servicoNome, servicoPreco);

            var itemId = Guid.NewGuid();
            var itemNome = "Filtro de óleo";
            var itemPreco = 35.00m;
            var itemQuantidade = 2;
            var itemTipo = global::Domain.OrdemServico.Enums.TipoItemIncluidoEnum.Peca;
            var itemData = new ItemData(itemId, itemNome, itemPreco, itemQuantidade, itemTipo);

            var ordemServico = new OrdemServicoBuilder()
                .ComServicos(servicoData)
                .ComItens(itemData)
                .Build();

            var presenter = new AdicionarItemPresenter();

            // Act
            presenter.ApresentarSucesso(ordemServico);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();

            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<OkObjectResult>();

            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeOfType<RetornoOrdemServicoComServicosItensDto>();

            var dto = okResult.Value as RetornoOrdemServicoComServicosItensDto;
            dto!.Id.Should().Be(ordemServico.Id);
            dto.Codigo.Should().Be(ordemServico.Codigo.Valor);
            dto.VeiculoId.Should().Be(ordemServico.VeiculoId);
            dto.Status.Should().Be(ordemServico.Status.Valor.ToString());
            dto.DataCriacao.Should().Be(ordemServico.Historico.DataCriacao);

            dto.ServicosIncluidos.Should().HaveCount(1);
            var servicoMapeado = dto.ServicosIncluidos[0];
            servicoMapeado.ServicoOriginalId.Should().Be(servicoId);
            servicoMapeado.Nome.Should().Be(servicoNome);
            servicoMapeado.Preco.Should().Be(servicoPreco);

            dto.ItensIncluidos.Should().HaveCount(1);
            var itemMapeado = dto.ItensIncluidos[0];
            itemMapeado.ItemEstoqueOriginalId.Should().Be(itemId);
            itemMapeado.Nome.Should().Be(itemNome);
            itemMapeado.Preco.Should().Be(itemPreco);
            itemMapeado.Quantidade.Should().Be(itemQuantidade);
            itemMapeado.TipoItemIncluido.Should().Be(itemTipo.ToString());
        }

        [Fact(DisplayName = "Deve retornar listas vazias quando serviços e itens forem nulos")]
        [Trait("Presenter", "AdicionarItemPresenter")]
        public void ApresentarSucesso_DeveRetornarListasVazias_QuandoServicosEItensForemNulos()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder().Build();
            var presenter = new AdicionarItemPresenter();

            // Act
            presenter.ApresentarSucesso(ordemServico);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();

            var resultado = presenter.ObterResultado();
            var okResult = resultado as OkObjectResult;
            var dto = okResult!.Value as RetornoOrdemServicoComServicosItensDto;

            dto!.ServicosIncluidos.Should().BeEmpty();
            dto.ItensIncluidos.Should().BeEmpty();
        }
    }
}
