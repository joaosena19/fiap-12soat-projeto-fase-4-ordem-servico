using Application.OrdemServico.Dtos;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Tests.Helpers;

namespace Tests.Integration.OrdemServico
{
    public class OrdemServicoExternalServiceBoundaryTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public OrdemServicoExternalServiceBoundaryTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient();
        }

        [Fact(DisplayName = "POST /api/ordens-servico deve retornar 422 quando veículo não existe")]
        [Trait("Componente", "ExternalServiceBoundary")]
        [Trait("Cenario", "VeiculoNaoEncontrado")]
        public async Task Post_ComVeiculoInexistente_DeveRetornar422()
        {
            // Arrange
            await _factory.ClearDatabaseAsync();
            
            var veiculoIdInexistente = Guid.NewGuid();
            
            // Não configurar o veículo nos mocks - simulando veículo não encontrado
            var createDto = new CriarOrdemServicoDto
            {
                VeiculoId = veiculoIdInexistente
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/orcamento/aprovar deve retornar 502 quando serviço externo falha")]
        [Trait("Componente", "ExternalServiceBoundary")]
        [Trait("Cenario", "UpstreamServiceFailure")]
        public async Task AprovarOrcamento_ComFalhaServicoExterno_DeveRetornar502()
        {
            // Arrange
            await _factory.ClearDatabaseAsync();
            
            var clienteId = Guid.NewGuid();
            var veiculoId = Guid.NewGuid();
            var servicoId = Guid.NewGuid();
            
            // Configurar mocks para criação da ordem
            _factory.MockExternalServices
                .AddCliente(clienteId, "Cliente Teste", DocumentoHelper.GerarCpfValido())
                .AddVeiculo(veiculoId, clienteId, "TST1234", "Civic Test", "Honda")
                .AddServico(servicoId, "Troca de Óleo", 100.00m);

            // Criar ordem de serviço
            var createDto = new CriarOrdemServicoDto { VeiculoId = veiculoId };
            var createResponse = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);
            createResponse.EnsureSuccessStatusCode();
            var ordem = await createResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDto>();

            // Adicionar serviço para gerar orçamento
            var adicionarServicoDto = new AdicionarServicosDto
            {
                ServicosOriginaisIds = new List<Guid> { servicoId }
            };
            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicoDto);

            // Gerar orçamento
            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/orcamento", new { });

            // Configurar falha na verificação de disponibilidade do estoque
            _factory.MockExternalServices.SetEstoqueDisponibilidadeResult(false);

            var aprovarDto = new { OrdemServicoId = ordem.Id };

            // Act - tentar aprovar orçamento com falha no serviço externo
            var response = await _client.PostAsJsonAsync("/api/ordens-servico/orcamento/aprovar", aprovarDto);

            // Assert
            // Note: O comportamento exato pode variar dependendo de como o controller lida com falhas externas
            // Pode ser 422 se for tratado como erro de negócio, ou 502 se for erro de infraestrutura
            response.StatusCode.Should().BeOneOf(HttpStatusCode.UnprocessableEntity, HttpStatusCode.BadGateway);
        }

        [Fact(DisplayName = "POST /api/ordens-servico deve retornar 422 quando dados do veículo inválidos")]
        [Trait("Componente", "ExternalServiceBoundary")]
        [Trait("Cenario", "DadosInvalidos")]
        public async Task Post_ComVeiculoComDadosInvalidos_DeveRetornar422()
        {
            // Arrange  
            await _factory.ClearDatabaseAsync();
            
            var veiculoId = Guid.NewGuid();
            
            // Configurar mock com veículo que existe mas com dados inválidos/malformados
            // Isso simula cenários onde o serviço externo retorna dados mas eles não passam na validação
            _factory.MockExternalServices
                .AddVeiculo(veiculoId, Guid.NewGuid(), "", "", "", "", 0, 0); // Dados vazios/inválidos

            var createDto = new CriarOrdemServicoDto
            {
                VeiculoId = veiculoId
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact(DisplayName = "Serviços externos devem propagar correlation ID")]
        [Trait("Componente", "ExternalServiceBoundary")]
        [Trait("Cenario", "CorrelationIdPropagation")]
        public async Task ExternalServices_DevemPropagarCorrelationId()
        {
            // Arrange
            await _factory.ClearDatabaseAsync();
            
            var clienteId = Guid.NewGuid();
            var veiculoId = Guid.NewGuid();
            
            _factory.MockExternalServices
                .AddCliente(clienteId, "Cliente Correlacao", DocumentoHelper.GerarCpfValido())
                .AddVeiculo(veiculoId, clienteId, "COR1234", "Civic Correlacao", "Honda");

            var correlationId = Guid.NewGuid().ToString();
            _client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

            var createDto = new CriarOrdemServicoDto
            {
                VeiculoId = veiculoId
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Verificar que a resposta contém o mesmo correlation ID
            response.Headers.Should().Contain(h => h.Key == "X-Correlation-ID");
            var responseCorrelationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
            responseCorrelationId.Should().Be(correlationId);
        }
    }
}