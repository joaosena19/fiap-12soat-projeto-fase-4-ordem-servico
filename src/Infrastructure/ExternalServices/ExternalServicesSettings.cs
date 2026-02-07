namespace Infrastructure.ExternalServices;

/// <summary>
/// Configurações para comunicação com serviços externos.
/// </summary>
public class ExternalServicesSettings
{
    /// <summary>
    /// URL base do serviço de Cadastros.
    /// </summary>
    public string CadastroBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL base do serviço de Estoque.
    /// </summary>
    public string EstoqueBaseUrl { get; set; } = string.Empty;
}
