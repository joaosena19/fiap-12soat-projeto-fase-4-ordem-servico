using Application.OrdemServico.Dtos.External;
using Application.OrdemServico.Interfaces.External;

namespace Infrastructure.ExternalServices.Stubs;

/// <summary>
/// Stub temporário para IVeiculoExternalService.
/// Será implementado com HTTP client na Phase D.
/// </summary>
public class VeiculoExternalServiceStub : IVeiculoExternalService
{
    public Task<bool> VerificarExistenciaVeiculo(Guid veiculoId)
    {
        throw new NotImplementedException("VeiculoExternalService será implementado na Phase D com HTTP client");
    }

    public Task<VeiculoExternalDto?> ObterVeiculoPorIdAsync(Guid veiculoId)
    {
        throw new NotImplementedException("VeiculoExternalService será implementado na Phase D com HTTP client");
    }

    public Task<VeiculoExternalDto?> ObterVeiculoPorPlacaAsync(string placa)
    {
        throw new NotImplementedException("VeiculoExternalService será implementado na Phase D com HTTP client");
    }

    public Task<VeiculoExternalDto> CriarVeiculoAsync(CriarVeiculoExternalDto dto)
    {
        throw new NotImplementedException("VeiculoExternalService será implementado na Phase D com HTTP client");
    }
}
