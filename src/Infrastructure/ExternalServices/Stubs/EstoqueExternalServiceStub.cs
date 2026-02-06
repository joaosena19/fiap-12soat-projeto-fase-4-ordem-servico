using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;

namespace Infrastructure.ExternalServices.Stubs;

/// <summary>
/// Stub temporário para IEstoqueExternalService.
/// Será implementado com HTTP client na Phase D.
/// </summary>
public class EstoqueExternalServiceStub : IEstoqueExternalService
{
    public Task<ItemEstoqueExternalDto?> ObterItemEstoquePorIdAsync(Guid itemId)
    {
        throw new NotImplementedException("EstoqueExternalService será implementado na Phase D com HTTP client");
    }

    public Task<bool> VerificarDisponibilidadeAsync(Guid itemId, int quantidadeNecessaria)
    {
        throw new NotImplementedException("EstoqueExternalService será implementado na Phase D com HTTP client");
    }

    public Task AtualizarQuantidadeEstoqueAsync(Guid itemId, int novaQuantidade)
    {
        throw new NotImplementedException("EstoqueExternalService será implementado na Phase D com HTTP client");
    }
}
