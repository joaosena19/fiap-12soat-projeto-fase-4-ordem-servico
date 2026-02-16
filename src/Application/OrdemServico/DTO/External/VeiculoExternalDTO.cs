namespace Application.OrdemServico.Dtos.External
{
    /// <summary>
    /// DTO para dados de Veículo vindos do bounded context de Cadastros
    /// </summary>
    public class VeiculoExternalDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public int Ano { get; set; }
       public int TipoVeiculo { get; set; } // Enum as int for loose coupling
    }

    /// <summary>
    /// DTO para criação de cliente no serviço de Cadastros
    /// </summary>
    public class CriarClienteExternalDto
    {
        public string Nome { get; set; } = string.Empty;
        public string DocumentoIdentificador { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para criação de veículo no serviço de Cadastros
    /// </summary>
    public class CriarVeiculoExternalDto
    {
        public Guid ClienteId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public int Ano { get; set; }
        public int TipoVeiculo { get; set; } // Enum as int for loose coupling
    }
}
