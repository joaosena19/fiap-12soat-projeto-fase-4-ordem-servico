namespace Application.OrdemServico.Dtos.External;

/// <summary>
/// DTO para retorno de verificação de disponibilidade de estoque de um serviço externo
/// </summary>
public class DisponibilidadeExternalDto
{
    /// <summary>
    /// Indica se o item está disponível na quantidade solicitada
    /// </summary>
    public bool Disponivel { get; set; }

    /// <summary>
    /// Quantidade atual em estoque
    /// </summary>
    public int QuantidadeEmEstoque { get; set; }

    /// <summary>
    /// Quantidade solicitada
    /// </summary>
    public int QuantidadeSolicitada { get; set; }
}
