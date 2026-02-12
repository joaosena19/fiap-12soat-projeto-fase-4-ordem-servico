using Application.OrdemServico.Dtos;
using Domain.OrdemServico.Enums;
using FluentAssertions;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Net;
using System.Net.Http.Json;
using Tests.Helpers;

namespace Tests.Integration.OrdemServico
{
    public class OrdemServicoControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public OrdemServicoControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        private IMongoCollection<global::Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico> GetCollection()
        {
            var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
            return context.OrdensServico;
        }

        #region Método Get Tests

        [Fact(DisplayName = "GET /api/ordens-servico deve retornar status 200 com lista vazia")]
        [Trait("Method", "Get")]
        public async Task Get_SemOrdensServico_DeveRetornarStatus200ComListaVazia()
        {
            await _factory.ClearDatabaseAsync();
            var response = await _client.GetAsync("/api/ordens-servico");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var ordens = await response.Content.ReadFromJsonAsync<List<RetornoOrdemServicoCompletaDto>>();
            ordens.Should().BeEmpty();
        }

        [Fact(DisplayName = "GET /api/ordens-servico deve retornar status 403 quando usuário não for administrador")]
        [Trait("Method", "Get")]
        public async Task Get_ComUsuarioNaoAdministrador_DeveRetornarStatus403()
        {
            var client = _factory.CreateAuthenticatedClient(isAdmin: false);
            var response = await client.GetAsync("/api/ordens-servico");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact(DisplayName = "GET /api/ordens-servico deve retornar status 200 com lista de ordens")]
        [Trait("Method", "Get")]
        public async Task Get_ComOrdensServico_DeveRetornarStatus200ComLista()
        {
            await _factory.ClearDatabaseAsync();
            var veiculoId = Guid.NewGuid();
            _factory.MockExternalServices.AddVeiculo(veiculoId, Guid.NewGuid(), "GET1234", "Modelo", "Marca");

            var dto = new CriarOrdemServicoDto { VeiculoId = veiculoId };
            var postResp = await _client.PostAsJsonAsync("/api/ordens-servico", dto);
            postResp.EnsureSuccessStatusCode();

            var response = await _client.GetAsync("/api/ordens-servico");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<RetornoOrdemServicoCompletaDto>>();
            list.Should().HaveCount(1);
        }

        #endregion

        #region Método GetById Tests

        [Fact(DisplayName = "GET /api/ordens-servico/{id} deve retornar status 200 quando ordem existir")]
        [Trait("Method", "GetById")]
        public async Task GetById_ComOrdemServicoExistente_DeveRetornarStatus200()
        {
            var veiculoId = Guid.NewGuid();
            _factory.MockExternalServices.AddVeiculo(veiculoId, Guid.NewGuid(), "GID1234", "Civic", "Honda");

            var dto = new CriarOrdemServicoDto { VeiculoId = veiculoId };
            var postResp = await _client.PostAsJsonAsync("/api/ordens-servico", dto);
            postResp.EnsureSuccessStatusCode();
            var created = await postResp.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();

            var response = await _client.GetAsync($"/api/ordens-servico/{created!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var returned = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoCompletaDto>();
            returned!.Id.Should().Be(created.Id);
        }

        [Fact(DisplayName = "GET /api/ordens-servico/{id} deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "GetById")]
        public async Task GetById_ComOrdemServicoInexistente_DeveRetornarStatus404()
        {
            var response = await _client.GetAsync($"/api/ordens-servico/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Método Post Tests

        [Fact(DisplayName = "POST /api/ordens-servico deve retornar status 201 com dados válidos")]
        [Trait("Method", "Post")]
        public async Task Post_ComDadosValidos_DeveRetornarStatus201()
        {
            var veiculoId = Guid.NewGuid();
            _factory.MockExternalServices.AddVeiculo(veiculoId, Guid.NewGuid(), "PST1234", "Civic", "Honda");

            var dto = new CriarOrdemServicoDto { VeiculoId = veiculoId };
            var response = await _client.PostAsJsonAsync("/api/ordens-servico", dto);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();
            created!.VeiculoId.Should().Be(veiculoId);
            created.Status.Should().Be("Recebida");

            var collection = GetCollection();
            var doc = await collection.Find(x => x.Id == created.Id).FirstOrDefaultAsync();
            doc.Should().NotBeNull();
        }

        [Fact(DisplayName = "POST /api/ordens-servico deve retornar status 422 com veículo inexistente")]
        [Trait("Method", "Post")]
        public async Task Post_ComVeiculoInexistente_DeveRetornarStatus422()
        {
            var dto = new CriarOrdemServicoDto { VeiculoId = Guid.NewGuid() };
            var response = await _client.PostAsJsonAsync("/api/ordens-servico", dto);
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region AdicionarServicos

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/servicos deve retornar status 200 com dados válidos")]
        [Trait("Method", "AdicionarServicos")]
        public async Task AdicionarServicos_ComDadosValidos_DeveRetornarStatus200()
        {
            var veiculoId = Guid.NewGuid();
            var servicoId = Guid.NewGuid();
            _factory.MockExternalServices
                .AddVeiculo(veiculoId, Guid.NewGuid(), "ASV1234", "Civic", "Honda")
                .AddServico(servicoId, "Troca Oleo", 100m);

            var createResp = await _client.PostAsJsonAsync("/api/ordens-servico", new CriarOrdemServicoDto { VeiculoId = veiculoId });
            var ordem = await createResp.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicDto = new AdicionarServicosDto { ServicosOriginaisIds = new List<Guid> { servicoId } };
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDto>();
            result!.ServicosIncluidos.Should().Contain(x => x.ServicoOriginalId == servicoId);
        }

        #endregion

        #region AdicionarItem

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/itens deve retornar status 200 com dados válidos")]
        [Trait("Method", "AdicionarItem")]
        public async Task AdicionarItem_ComDadosValidos_DeveRetornarStatus200()
        {
            var veiculoId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            _factory.MockExternalServices
                .AddVeiculo(veiculoId, Guid.NewGuid(), "AIT1234", "Civic", "Honda")
                .AddProduto(itemId, "Peca X", 50m);

            var createResp = await _client.PostAsJsonAsync("/api/ordens-servico", new CriarOrdemServicoDto { VeiculoId = veiculoId });
            var ordem = await createResp.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicDto = new AdicionarItemDto { ItemEstoqueOriginalId = itemId, Quantidade = 2 };
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDto>();
            result!.ItensIncluidos.Should().Contain(x => x.ItemEstoqueOriginalId == itemId && x.Quantidade == 2);
        }

        #endregion

        #region AprovarOrcamento

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/aprovar deve retornar status 204 e mudar status para EmExecucao")]
        [Trait("Method", "AprovarOrcamento")]
        public async Task AprovarOrcamento_ComDadosValidos_DeveRetornarStatus204()
        {
            var veiculoId = Guid.NewGuid();
            var servicoId = Guid.NewGuid();
            _factory.MockExternalServices
                .AddVeiculo(veiculoId, Guid.NewGuid(), "APR1234", "Civic", "Honda")
                .AddServico(servicoId, "Servico A", 200m);

            // Cria OS
            var createResp = await _client.PostAsJsonAsync("/api/ordens-servico", new CriarOrdemServicoDto { VeiculoId = veiculoId });
            var ordem = await createResp.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();
            
            // Inicia Diagnostico
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            // Add Servico
            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", new AdicionarServicosDto { ServicosOriginaisIds = new List<Guid> { servicoId } });

            // Gera Orcamento
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Aprova
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var collection = GetCollection();
            var doc = await collection.Find(x => x.Id == ordem.Id).FirstOrDefaultAsync();
            doc.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/aprovar deve falhar com 422 se estoque insuficiente")]
        [Trait("Method", "AprovarOrcamento")]
        public async Task AprovarOrcamento_EstoqueInsuficiente_DeveRetornar422()
        {
             var veiculoId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            _factory.MockExternalServices
                .AddVeiculo(veiculoId, Guid.NewGuid(), "INS1234", "Civic", "Honda")
                .AddProduto(itemId, "Peca Rara", 50m)
                .SetEstoqueDisponibilidadeResult(false); // Força indisponibilidade

            var createResp = await _client.PostAsJsonAsync("/api/ordens-servico", new CriarOrdemServicoDto { VeiculoId = veiculoId });
            var ordem = await createResp.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", new AdicionarItemDto { ItemEstoqueOriginalId = itemId, Quantidade = 1 });
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion
        
        #region Cancelar

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/cancelar deve retornar status 204")]
        [Trait("Method", "Cancelar")]
        public async Task Cancelar_ComDadosValidos_DeveRetornarStatus204()
        {
            var veiculoId = Guid.NewGuid();
            _factory.MockExternalServices.AddVeiculo(veiculoId, Guid.NewGuid(), "CAN1234", "Civic", "Honda");

            var createResp = await _client.PostAsJsonAsync("/api/ordens-servico", new CriarOrdemServicoDto { VeiculoId = veiculoId });
            var ordem = await createResp.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();

            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/cancelar", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var collection = GetCollection();
            var doc = await collection.Find(x => x.Id == ordem.Id).FirstOrDefaultAsync();
            doc.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
        }

        #endregion
    }
}
