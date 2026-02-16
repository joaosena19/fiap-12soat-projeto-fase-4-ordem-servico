using API.Presenters.OrdemServico;
using Application.OrdemServico.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.Application.SharedHelpers.AggregateBuilders;
using Xunit;

namespace Tests.API.Presenters.OrdemServico
{
    public class BuscaPublicaOrdemServicoPresenterTests
    {
        [Fact(DisplayName = "Deve mapear DTO completo com orçamento quando ordem de serviço tem orçamento")]
        [Trait("Presenter", "BuscaPublicaOrdemServicoPresenter")]
        public void ApresentarSucesso_DeveMapearOrcamento_QuandoExistir()
        {
            // Arrange
            var servicoId = Guid.NewGuid();
            var servicoNome = "Revisão completa";
            var servicoPreco = 300.00m;
            var servicoData = new ServicoData(servicoId, servicoNome, servicoPreco);

            var itemId = Guid.NewGuid();
            var itemNome = "Óleo sintético";
            var itemPreco = 80.00m;
            var itemQuantidade = 3;
            var itemTipo = global::Domain.OrdemServico.Enums.TipoItemIncluidoEnum.Peca;
            var itemData = new ItemData(itemId, itemNome, itemPreco, itemQuantidade, itemTipo);

            var ordemServico = new OrdemServicoBuilder()
                .ComServicos(servicoData)
                .ComItens(itemData)
                .ComOrcamento(comItens: false)
                .Build();

            var presenter = new BuscaPublicaOrdemServicoPresenter();

            // Act
            presenter.ApresentarSucesso(ordemServico);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();

            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<OkObjectResult>();

            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeOfType<RetornoOrdemServicoCompletaDto>();

            var dto = okResult.Value as RetornoOrdemServicoCompletaDto;
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

            dto.Orcamento.Should().NotBeNull();
            dto.Orcamento!.Id.Should().Be(ordemServico.Orcamento!.Id);
            dto.Orcamento.Preco.Should().Be(ordemServico.Orcamento.Preco.Valor);
            dto.Orcamento.DataCriacao.Should().Be(ordemServico.Orcamento.DataCriacao.Valor);
        }

        [Fact(DisplayName = "Deve permitir orçamento nulo quando ordem de serviço não tem orçamento")]
        [Trait("Presenter", "BuscaPublicaOrdemServicoPresenter")]
        public void ApresentarSucesso_DevePermitirOrcamentoNulo_QuandoNaoExistir()
        {
            // Arrange
            var servicoData = new ServicoData(Guid.NewGuid(), "Diagnóstico", 100.00m);
            var itemData = new ItemData(Guid.NewGuid(), "Parafuso", 5.00m, 10, global::Domain.OrdemServico.Enums.TipoItemIncluidoEnum.Peca);

            var ordemServico = new OrdemServicoBuilder()
                .ComServicos(servicoData)
                .ComItens(itemData)
                .Build();

            var presenter = new BuscaPublicaOrdemServicoPresenter();

            // Act
            presenter.ApresentarSucesso(ordemServico);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();

            var resultado = presenter.ObterResultado();
            var okResult = resultado as OkObjectResult;
            var dto = okResult!.Value as RetornoOrdemServicoCompletaDto;

            dto!.Orcamento.Should().BeNull();
        }

        [Fact(DisplayName = "Deve retornar listas vazias quando serviços e itens forem nulos")]
        [Trait("Presenter", "BuscaPublicaOrdemServicoPresenter")]
        public void ApresentarSucesso_DeveRetornarListasVazias_QuandoServicosEItensForemNulos()
        {
            // Arrange
            var ordemServico = new OrdemServicoBuilder().Build();
            var presenter = new BuscaPublicaOrdemServicoPresenter();

            // Act
            presenter.ApresentarSucesso(ordemServico);

            // Assert
            presenter.FoiSucesso.Should().BeTrue();

            var resultado = presenter.ObterResultado();
            var okResult = resultado as OkObjectResult;
            var dto = okResult!.Value as RetornoOrdemServicoCompletaDto;

            dto!.ServicosIncluidos.Should().BeEmpty();
            dto.ItensIncluidos.Should().BeEmpty();
            dto.Orcamento.Should().BeNull();
        }

        [Fact(DisplayName = "Deve retornar resultado com status 200 e valor nulo quando não encontrado")]
        [Trait("Presenter", "BuscaPublicaOrdemServicoPresenter")]
        public void ApresentarNaoEncontrado_DeveRetornarResultadoComValorNulo_QuandoChamado()
        {
            // Arrange
            var presenter = new BuscaPublicaOrdemServicoPresenter();

            // Act
            presenter.ApresentarNaoEncontrado();

            // Assert
            presenter.FoiSucesso.Should().BeTrue();

            var resultado = presenter.ObterResultado();
            resultado.Should().BeOfType<JsonResult>();

            var jsonResult = resultado as JsonResult;
            jsonResult!.StatusCode.Should().Be(200);
            jsonResult.Value.Should().BeNull();
        }
    }
}
