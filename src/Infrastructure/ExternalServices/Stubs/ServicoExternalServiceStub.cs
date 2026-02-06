using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;

namespace Infrastructure.ExternalServices.Stubs;

/// <summary>
/// Stub temporário para IServicoExternalService.
/// Será implementado com HTTP client na Phase D.
/// </summary>
public class ServicoExternalServiceStub : IServicoExternalService
{
    public Task<ServicoExternalDto?> ObterServicoPorIdAsync(Guid servicoId)
    {
        throw new NotImplementedException("ServicoExternalService será implementado na Phase D com HTTP client");
    }
}
