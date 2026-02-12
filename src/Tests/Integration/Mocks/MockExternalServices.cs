using Application.OrdemServico.Interfaces.External;
using Application.OrdemServico.Dtos.External;
using Moq;
using System.Collections.Concurrent;

namespace Tests.Integration.Mocks;

public class MockExternalServices
{
    private readonly Mock<IClienteExternalService> _clienteMock;
    private readonly Mock<IVeiculoExternalService> _veiculoMock;
    private readonly Mock<IServicoExternalService> _servicoMock;
    private readonly Mock<IEstoqueExternalService> _estoqueMock;

    // Armazenamento configurável para retornos dos mocks
    private readonly ConcurrentDictionary<Guid, ClienteExternalDto> _clientes = new();
    private readonly ConcurrentDictionary<Guid, VeiculoExternalDto> _veiculos = new();
    private readonly ConcurrentDictionary<Guid, ServicoExternalDto> _servicos = new();
    private readonly ConcurrentDictionary<Guid, ItemEstoqueExternalDto> _produtos = new();

    public MockExternalServices()
    {
        _clienteMock = new Mock<IClienteExternalService>();
        _veiculoMock = new Mock<IVeiculoExternalService>();
        _servicoMock = new Mock<IServicoExternalService>();
        _estoqueMock = new Mock<IEstoqueExternalService>();

        ConfigureMocks();
    }

    public IClienteExternalService ClienteService => _clienteMock.Object;
    public IVeiculoExternalService VeiculoService => _veiculoMock.Object;
    public IServicoExternalService ServicoService => _servicoMock.Object;
    public IEstoqueExternalService EstoqueService => _estoqueMock.Object;

    private void ConfigureMocks()
    {
        // Cliente mock setup
        _clienteMock.Setup(x => x.ObterClientePorVeiculoIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => _clientes.Values.FirstOrDefault(c => _veiculos.Values.Any(v => v.Id == id && v.ClienteId == c.Id)));

        // Veículo mock setup  
        _veiculoMock.Setup(x => x.ObterVeiculoPorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => _veiculos.TryGetValue(id, out var veiculo) ? veiculo : null);

        _veiculoMock.Setup(x => x.VerificarExistenciaVeiculo(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => _veiculos.ContainsKey(id));

        // Serviço mock setup
        _servicoMock.Setup(x => x.ObterServicoPorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => _servicos.TryGetValue(id, out var servico) ? servico : null);

        // Estoque mock setup
        _estoqueMock.Setup(x => x.ObterItemEstoquePorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => _produtos.TryGetValue(id, out var produto) ? produto : null);

        _estoqueMock.Setup(x => x.VerificarDisponibilidadeAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true); // Default: disponível
    }

    // Métodos para configurar retornos nos testes
    public MockExternalServices AddCliente(Guid id, string nome, string documento)
    {
        _clientes[id] = new ClienteExternalDto 
        { 
            Id = id, 
            Nome = nome, 
            DocumentoIdentificador = documento 
        };
        return this;
    }

    public MockExternalServices AddVeiculo(Guid id, Guid clienteId, string placa, string modelo, string marca, string cor = "Branco", int ano = 2020, int tipoVeiculo = 1)
    {
        _veiculos[id] = new VeiculoExternalDto
        {
            Id = id,
            ClienteId = clienteId,
            Placa = placa,
            Modelo = modelo,
            Marca = marca,
            Cor = cor,
            Ano = ano,
            TipoVeiculo = tipoVeiculo
        };
        return this;
    }

    public MockExternalServices AddServico(Guid id, string nome, decimal preco)
    {
        _servicos[id] = new ServicoExternalDto
        {
            Id = id,
            Nome = nome,
            Preco = preco
        };
        return this;
    }

    public MockExternalServices AddProduto(Guid id, string nome, decimal preco, int quantidade = 10)
    {
        _produtos[id] = new ItemEstoqueExternalDto
        {
            Id = id,
            Nome = nome,
            Preco = preco,
            Quantidade = quantidade,
            TipoItemIncluido = global::Domain.OrdemServico.Enums.TipoItemIncluidoEnum.Peca // Default tipo peca
        };
        return this;
    }

    // Método para configurar falhas de estoque
    public MockExternalServices SetEstoqueDisponibilidadeResult(bool disponivel)
    {
        _estoqueMock.Setup(x => x.VerificarDisponibilidadeAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(disponivel);
        return this;
    }

    // Método para limpar todos os dados entre testes
    public void Clear()
    {
        _clientes.Clear();
        _veiculos.Clear(); 
        _servicos.Clear();
        _produtos.Clear();
        
        // Reset estoque para comportamento padrão
        SetEstoqueDisponibilidadeResult(true);
    }
}