using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;

namespace Infrastructure.ExternalServices.Stubs;

/// <summary>
/// Stub temporário para IClienteExternalService.
/// Será implementado com HTTP client na Phase D.
/// </summary>
public class ClienteExternalServiceStub : IClienteExternalService
{
    public Task<ClienteExternalDto?> ObterClientePorVeiculoIdAsync(Guid veiculoId)
    {
        throw new NotImplementedException("ClienteExternalService será implementado na Phase D com HTTP client");
    }

    public Task<ClienteExternalDto?> ObterClientePorDocumentoAsync(string documentoIdentificador)
    {
        throw new NotImplementedException("ClienteExternalService será implementado na Phase D com HTTP client");
    }

    public Task<ClienteExternalDto> CriarClienteAsync(CriarClienteExternalDto dto)
    {
        throw new NotImplementedException("ClienteExternalService será implementado na Phase D com HTTP client");
    }
}
