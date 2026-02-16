using Application.OrdemServico.Dtos;
using Domain.OrdemServico.Enums;
using Bogus;

namespace Tests.Integration.OrdemServico.Builders;

/// <summary>
/// Builder para criar DTOs de requisições da API OrdemServico para testes de integração
/// </summary>
public static class OrdemServicoRequestBuilder
{
    private static readonly Faker _faker = new("pt_BR");

    /// <summary>
    /// Cria uma requisição básica de criação de ordem de serviço
    /// </summary>
    public static CriarOrdemServicoDto CriarOrdemServicoBasica()
    {
        return new CriarOrdemServicoDto
        {
            VeiculoId = Guid.NewGuid()
        };
    }

    /// <summary>
    /// Cria uma requisição básica de criação completa de ordem de serviço
    /// </summary>
    public static CriarOrdemServicoCompletaDto CriarOrdemServicoCompleta()
    {
        return new CriarOrdemServicoCompletaDto
        {
            Cliente = new ClienteDto
            {
                Nome = _faker.Person.FullName,
                DocumentoIdentificador = _faker.Random.Replace("###.###.###-##")
            },
            Veiculo = new VeiculoDto
            {
                Placa = _faker.Random.Replace("###-####"),
                Modelo = _faker.Vehicle.Model(),
                Marca = _faker.Vehicle.Manufacturer(),
                Cor = _faker.Commerce.Color(),
                Ano = _faker.Random.Int(2000, DateTime.Now.Year),
                TipoVeiculo = _faker.PickRandom<TipoVeiculoEnum>()
            },
            ServicosIds = new List<Guid>(),
            Itens = new List<ItemDto>()
        };
    }

    /// <summary>
    /// Cria uma requisição para adicionar serviços
    /// </summary>
    public static AdicionarServicosDto AdicionarServicos(params Guid[] servicoIds)
    {
        return new AdicionarServicosDto
        {
            ServicosOriginaisIds = servicoIds?.ToList() ?? new List<Guid> { Guid.NewGuid() }
        };
    }

    /// <summary>
    /// Cria uma requisição para adicionar itens
    /// </summary>
    public static AdicionarItemDto AdicionarItem()
    {
        return new AdicionarItemDto
        {
            ItemEstoqueOriginalId = Guid.NewGuid(),
            Quantidade = _faker.Random.Int(1, 10)
        };
    }

    /// <summary>
    /// Cria uma requisição de busca pública
    /// </summary>
    public static BuscaPublicaOrdemServicoDto BuscaPublica(string? documento = null, string? codigo = null)
    {
        return new BuscaPublicaOrdemServicoDto
        {
            DocumentoIdentificadorCliente = documento ?? _faker.Random.Replace("###.###.###-##"),
            CodigoOrdemServico = codigo ?? $"OS-{_faker.Random.Int(1000, 9999)}"
        };
    }

    /// <summary>
    /// Cria uma requisição de webhook de alteração de status
    /// </summary>
    public static WebhookAlterarStatusDto WebhookAlterarStatus(Guid ordemServicoId, StatusOrdemServicoEnum status = StatusOrdemServicoEnum.Finalizada)
    {
        return new WebhookAlterarStatusDto
        {
            Id = ordemServicoId,
            Status = status
        };
    }

    /// <summary>
    /// Cria uma requisição de webhook com ID
    /// </summary>
    public static WebhookIdDto WebhookComId(Guid ordemServicoId)
    {
        return new WebhookIdDto
        {
            Id = ordemServicoId
        };
    }
}