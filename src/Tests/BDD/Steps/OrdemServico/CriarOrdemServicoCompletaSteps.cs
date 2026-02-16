using Application.OrdemServico.Dtos;
using Application.OrdemServico.Dtos.External;
using Domain.OrdemServico.Enums;
using FluentAssertions;
using Moq;
using Reqnroll;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tests.BDD.Support.OrdemServico;
using Tests.Integration.Helpers;
using Tests.Integration.OrdemServico.Builders;

namespace Tests.BDD.Steps.OrdemServico
{
    [Binding]
    public class CriarOrdemServicoCompletaSteps
    {
        private readonly CriarOrdemServicoCompletaContexto _contexto;

        public CriarOrdemServicoCompletaSteps(CriarOrdemServicoCompletaContexto contexto)
        {
            _contexto = contexto;
        }

        [Given(@"que o sistema de ordens de serviço está operacional")]
        public void DadoQueOSistemaEstaOperacional()
        {
            _contexto.Factory.Should().NotBeNull();
        }

        [Given(@"que sou um administrador autenticado")]
        public void DadoQueSouAdministrador()
        {
            _contexto.Client = _contexto.Factory.CreateAuthenticatedClient(isAdmin: true);
        }

        [Given(@"que sou um cliente comum autenticado")]
        public void DadoQueSouClienteComum()
        {
            _contexto.Client = _contexto.Factory.CreateAuthenticatedClient(isAdmin: false, clienteId: Guid.NewGuid());
        }

        [Given(@"que o cliente informado ainda não possui cadastro")]
        public void DadoQueClienteNaoPossuiCadastro()
        {
            _contexto.Factory.Mocks.ClienteService
                .Setup(x => x.ObterPorDocumentoAsync(It.IsAny<string>()))
                .ReturnsAsync((ClienteExternalDto?)null);
        }

        [Given(@"que o veículo informado ainda não possui cadastro")]
        public void DadoQueVeiculoNaoPossuiCadastro()
        {
            _contexto.Factory.Mocks.VeiculoService
                .Setup(x => x.ObterVeiculoPorPlacaAsync(It.IsAny<string>()))
                .ReturnsAsync((VeiculoExternalDto?)null);
        }

        [Given(@"que o sistema deve cadastrar o novo cliente automaticamente")]
        public void DadoQueSistemaCadastraNovoCliente()
        {
            _contexto.ClienteIdExistente = Guid.NewGuid();
            _contexto.Factory.Mocks.ClienteService
                .Setup(x => x.CriarClienteAsync(It.IsAny<CriarClienteExternalDto>()))
                .ReturnsAsync(new ClienteExternalDto
                {
                    Id = _contexto.ClienteIdExistente,
                    Nome = "Cliente Teste",
                    DocumentoIdentificador = "123.456.789-00"
                });
        }

        [Given(@"que o sistema deve cadastrar o novo veículo automaticamente")]
        public void DadoQueSistemaCadastraNovoVeiculo()
        {
            _contexto.VeiculoIdExistente = Guid.NewGuid();
            _contexto.Factory.Mocks.VeiculoService
                .Setup(x => x.CriarVeiculoAsync(It.IsAny<CriarVeiculoExternalDto>()))
                .ReturnsAsync(new VeiculoExternalDto
                {
                    Id = _contexto.VeiculoIdExistente,
                    ClienteId = _contexto.ClienteIdExistente,
                    Placa = "ABC-1234",
                    Modelo = "Modelo Teste",
                    Marca = "Marca Teste",
                    Cor = "Preto",
                    Ano = 2023,
                    TipoVeiculo = "Carro"
                });
        }

        [Given(@"que o cliente informado já possui cadastro")]
        public void DadoQueClienteJaPossuiCadastro()
        {
            _contexto.ClienteIdExistente = Guid.NewGuid();
            _contexto.Factory.Mocks.ClienteService
                .Setup(x => x.ObterPorDocumentoAsync(It.IsAny<string>()))
                .ReturnsAsync(new ClienteExternalDto
                {
                    Id = _contexto.ClienteIdExistente,
                    Nome = "Cliente Existente",
                    DocumentoIdentificador = "987.654.321-00"
                });
        }

        [Given(@"que o veículo informado já possui cadastro")]
        public void DadoQueVeiculoJaPossuiCadastro()
        {
            _contexto.VeiculoIdExistente = Guid.NewGuid();
            _contexto.Factory.Mocks.VeiculoService
                .Setup(x => x.ObterVeiculoPorPlacaAsync(It.IsAny<string>()))
                .ReturnsAsync(new VeiculoExternalDto
                {
                    Id = _contexto.VeiculoIdExistente,
                    ClienteId = _contexto.ClienteIdExistente,
                    Placa = "XYZ-9876",
                    Modelo = "Modelo Existente",
                    Marca = "Marca Existente",
                    Cor = "Branco",
                    Ano = 2022,
                    TipoVeiculo = "Carro"
                });
        }

        [Given(@"que o sistema não deve duplicar o cadastro do cliente")]
        public void DadoQueNaoDeveDuplicarCliente()
        {
            _contexto.Factory.Mocks.ClienteService
                .Setup(x => x.CriarClienteAsync(It.IsAny<CriarClienteExternalDto>()))
                .ThrowsAsync(new InvalidOperationException("Não deveria tentar criar cliente quando já existe"));
        }

        [Given(@"que o sistema não deve duplicar o cadastro do veículo")]
        public void DadoQueNaoDeveDuplicarVeiculo()
        {
            _contexto.Factory.Mocks.VeiculoService
                .Setup(x => x.CriarVeiculoAsync(It.IsAny<CriarVeiculoExternalDto>()))
                .ThrowsAsync(new InvalidOperationException("Não deveria tentar criar veículo quando já existe"));
        }

        [Given(@"que o cliente e o veículo já possuem cadastro")]
        public void DadoQueClienteEVeiculoJaPossuemCadastro()
        {
            _contexto.ClienteIdExistente = Guid.NewGuid();
            _contexto.VeiculoIdExistente = Guid.NewGuid();

            _contexto.Factory.Mocks.ClienteService
                .Setup(x => x.ObterPorDocumentoAsync(It.IsAny<string>()))
                .ReturnsAsync(new ClienteExternalDto
                {
                    Id = _contexto.ClienteIdExistente,
                    Nome = "Cliente Teste",
                    DocumentoIdentificador = "111.222.333-44"
                });

            _contexto.Factory.Mocks.VeiculoService
                .Setup(x => x.ObterVeiculoPorPlacaAsync(It.IsAny<string>()))
                .ReturnsAsync(new VeiculoExternalDto
                {
                    Id = _contexto.VeiculoIdExistente,
                    ClienteId = _contexto.ClienteIdExistente,
                    Placa = "TST-1234",
                    Modelo = "Modelo",
                    Marca = "Marca",
                    Cor = "Azul",
                    Ano = 2023,
                    TipoVeiculo = "Carro"
                });
        }

        [Given(@"que informei (\d+) serviços mas apenas (\d+) existe no catálogo")]
        public void DadoQueInformeiServicosComApenasUmExistente(int total, int existentes)
        {
            _contexto.ServicoIdExistente = Guid.NewGuid();
            _contexto.ServicoIdInexistente = Guid.NewGuid();

            _contexto.Factory.Mocks.ServicoService
                .Setup(x => x.ObterServicoPorIdAsync(_contexto.ServicoIdExistente))
                .ReturnsAsync(new ServicoExternalDto
                {
                    Id = _contexto.ServicoIdExistente,
                    Nome = "Serviço Existente",
                    Preco = 100.00m
                });

            _contexto.Factory.Mocks.ServicoService
                .Setup(x => x.ObterServicoPorIdAsync(_contexto.ServicoIdInexistente))
                .ReturnsAsync((ServicoExternalDto?)null);
        }

        [Given(@"que informei (\d+) itens de estoque mas apenas (\d+) existe no estoque")]
        public void DadoQueInformeiItensComApenasUmExistente(int total, int existentes)
        {
            _contexto.ItemEstoqueIdExistente = Guid.NewGuid();
            _contexto.ItemEstoqueIdInexistente = Guid.NewGuid();

            _contexto.Factory.Mocks.EstoqueService
                .Setup(x => x.ObterItemEstoquePorIdAsync(_contexto.ItemEstoqueIdExistente))
                .ReturnsAsync(new ItemEstoqueExternalDto
                {
                    Id = _contexto.ItemEstoqueIdExistente,
                    Nome = "Item Existente",
                    Preco = 50.00m,
                    Quantidade = 10,
                    TipoItemEstoque = "Peca"
                });

            _contexto.Factory.Mocks.EstoqueService
                .Setup(x => x.ObterItemEstoquePorIdAsync(_contexto.ItemEstoqueIdInexistente))
                .ReturnsAsync((ItemEstoqueExternalDto?)null);
        }

        [When(@"eu solicitar a criação da ordem de serviço completa")]
        public async Task QuandoSolicitarCriacao()
        {
            _contexto.Request = OrdemServicoRequestBuilder.CriarOrdemServicoCompleta();

            if (_contexto.ServicoIdExistente != Guid.Empty)
            {
                _contexto.Request.ServicosIds = new List<Guid>
                {
                    _contexto.ServicoIdExistente,
                    _contexto.ServicoIdInexistente
                };
            }

            if (_contexto.ItemEstoqueIdExistente != Guid.Empty)
            {
                _contexto.Request.Itens = new List<ItemDto>
                {
                    new ItemDto { ItemEstoqueId = _contexto.ItemEstoqueIdExistente, Quantidade = 2 },
                    new ItemDto { ItemEstoqueId = _contexto.ItemEstoqueIdInexistente, Quantidade = 3 }
                };
            }

            _contexto.Response = await _contexto.Client.PostAsJsonAsync("/api/ordens-servico/completa", _contexto.Request);
        }

        [Then(@"a ordem de serviço deve ser registrada com sucesso")]
        public async Task EntaoOrdemDeveSerRegistradaComSucesso()
        {
            _contexto.Response.StatusCode.Should().Be(HttpStatusCode.Created);

            var content = await _contexto.Response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var id = json.RootElement.GetProperty("id").GetString();
            _contexto.OrdemServicoIdCriada = Guid.Parse(id!);
        }

        [Then(@"o sistema deve informar onde acessar a ordem criada")]
        public void EntaoSistemaDeveInformarOndeAcessar()
        {
            _contexto.Response.Headers.Location.Should().NotBeNull();
        }

        [Then(@"o status inicial deve ser ""(.*)""")]
        public async Task EntaoStatusInicialDeveSer(string statusEsperado)
        {
            var content = await _contexto.Response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var status = json.RootElement.GetProperty("status").GetString();
            status.Should().Be(statusEsperado);
        }

        [Then(@"um código identificador deve ser gerado com prefixo ""(.*)""")]
        public async Task EntaoCodigoDeveSerGeradoComPrefixo(string prefixo)
        {
            var content = await _contexto.Response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var codigo = json.RootElement.GetProperty("codigo").GetString();
            codigo.Should().StartWith(prefixo);
        }

        [Then(@"o acesso deve ser negado")]
        public void EntaoAcessoDeveSerNegado()
        {
            _contexto.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Then(@"o sistema deve informar que apenas administradores podem criar ordens de serviço completas")]
        public async Task EntaoSistemaDeveInformarQueApenasAdministradores()
        {
            var content = await _contexto.Response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var mensagem = json.RootElement.GetProperty("message").GetString();
            mensagem.Should().Be("Acesso negado. Apenas administradores podem criar ordens de serviço completas.");
        }

        [Then(@"a ordem de serviço deve estar registrada no sistema")]
        public async Task EntaoOrdemDeveEstarRegistradaNoSistema()
        {
            _contexto.OrdemServicoIdCriada.Should().NotBeNull();

            var ordemServico = await MongoOrdemServicoTestHelper.FindByIdAsync(_contexto.Factory.Services, _contexto.OrdemServicoIdCriada!.Value);
            ordemServico.Should().NotBeNull();
            ordemServico!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Recebida);
        }

        [Then(@"apenas os serviços válidos devem ser incluídos na ordem")]
        public async Task EntaoApenasServicosValidosDevemSerIncluidos()
        {
            _contexto.OrdemServicoIdCriada.Should().NotBeNull();

            var ordemServico = await MongoOrdemServicoTestHelper.FindByIdAsync(_contexto.Factory.Services, _contexto.OrdemServicoIdCriada!.Value);
            ordemServico.Should().NotBeNull();
            ordemServico!.ServicosIncluidos.Should().HaveCount(1);
        }

        [Then(@"apenas os itens válidos devem ser incluídos na ordem")]
        public async Task EntaoApenasItensValidosDevemSerIncluidos()
        {
            _contexto.OrdemServicoIdCriada.Should().NotBeNull();

            var ordemServico = await MongoOrdemServicoTestHelper.FindByIdAsync(_contexto.Factory.Services, _contexto.OrdemServicoIdCriada!.Value);
            ordemServico.Should().NotBeNull();
            ordemServico!.ItensIncluidos.Should().HaveCount(1);
        }
    }
}
