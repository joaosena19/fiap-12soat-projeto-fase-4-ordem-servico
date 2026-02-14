using API.Presenters.OrdemServico;
using Application.OrdemServico.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.Application.SharedHelpers.AggregateBuilders;
using Xunit;

namespace Tests.API.Presenters.OrdemServico
{
    public class BuscarOrdemServicoPorIdPresenterTests
    {
        [Fact(DisplayName = "Deve mapear serviços, itens e sem orçamento quando ordem de serviço não tem orçamento")]
        [Trait("Presenter", "BuscarOrdemServicoPorId")]
        public void ApresentarSucesso_DeveMapearServicosItensESemOrcamento_QuandoOrdemServicoNaoTemOrcamento()
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

            var presenter = new BuscarOrdemServicoPorIdPresenter();

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

            dto.Orcamento.Should().BeNull();
        }

        [Fact(DisplayName = "Deve mapear serviços, itens e orçamento quando ordem de serviço tem orçamento")]
        [Trait("Presenter", "BuscarOrdemServicoPorId")]
        public void ApresentarSucesso_DeveMapearServicosItensEOrcamento_QuandoOrdemServicoTemOrcamento()
        {
            // Arrange
            var servicoId = Guid.NewGuid();
            var servicoNome = "Alinhamento e balanceamento";
            var servicoPreco = 200.00m;
            var servicoData = new ServicoData(servicoId, servicoNome, servicoPreco);

            var itemId = Guid.NewGuid();
            var itemNome = "Pastilha de freio";
            var itemPreco = 120.00m;
            var itemQuantidade = 4;
            var itemTipo = global::Domain.OrdemServico.Enums.TipoItemIncluidoEnum.Peca;
            var itemData = new ItemData(itemId, itemNome, itemPreco, itemQuantidade, itemTipo);

            var ordemServico = new OrdemServicoBuilder()
                .ComServicos(servicoData)
                .ComItens(itemData)
                .ComOrcamento(comItens: false)
                .Build();

            var presenter = new BuscarOrdemServicoPorIdPresenter();

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
    }
}
