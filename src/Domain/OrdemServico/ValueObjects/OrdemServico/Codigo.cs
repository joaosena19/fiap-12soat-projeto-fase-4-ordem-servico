using Shared.Exceptions;
using Shared.Enums;
using System.Text.RegularExpressions;
using Shared.Attributes;

namespace Domain.OrdemServico.ValueObjects.OrdemServico
{
    [ValueObject]
    public record Codigo
    {
        private string _valor = string.Empty;

        // Construtor sem parâmetro para o EF Core
        private Codigo() { }

        public Codigo(string codigo)
        {
            codigo = codigo?.Trim().ToUpper() ?? string.Empty;
            var _regex = new Regex(@"^OS-\d{8}-[A-Z0-9]{6}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

            if (string.IsNullOrWhiteSpace(codigo) || !_regex.IsMatch(codigo))
                throw new DomainException($"Código {codigo} inválido. Formato esperado: OS-YYYYMMDD-ABC123", ErrorType.InvalidInput);

            _valor = codigo;
        }

        public static Codigo GerarNovo()
        {
            var data = DateTime.UtcNow.ToString("yyyyMMdd");
            var sufixo = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(); // 6 primeiros chars
            var codigo = $"OS-{data}-{sufixo}";
            return new Codigo(codigo);
        }

        /// <summary>
        /// Reidrata o Codigo a partir de dados do banco SEM VALIDAÇÃO.
        /// NÃO deve ser usado fora do contexto de buscar do banco.
        /// </summary>
        public static Codigo Reidratar(string codigo)
        {
            return new Codigo { _valor = codigo };
        }

        public string Valor => _valor;

    }
}
