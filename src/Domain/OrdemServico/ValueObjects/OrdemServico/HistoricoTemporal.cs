using Shared.Attributes;
using Shared.Enums;
using Shared.Exceptions;

namespace Domain.OrdemServico.ValueObjects.OrdemServico
{
    [ValueObject]
    public record HistoricoTemporal
    {
        private DateTime _dataCriacao;
        private DateTime? _dataInicioExecucao;
        private DateTime? _dataFinalizacao;
        private DateTime? _dataEntrega;

        // Construtor sem parâmetros para o EF Core
        private HistoricoTemporal() { }

        public HistoricoTemporal(DateTime dataCriacao, DateTime? dataInicioExecucao = null, DateTime? dataFinalizacao = null, DateTime? dataEntrega = null)
        {
            if (dataCriacao == default)
                throw new DomainException("A data de criação é obrigatória.", ErrorType.InvalidInput);

            if (dataInicioExecucao.HasValue && dataInicioExecucao < dataCriacao)
                throw new DomainException("A data de início de execução não pode ser anterior à data de criação.", ErrorType.DomainRuleBroken);

            if (dataFinalizacao.HasValue && (!dataInicioExecucao.HasValue || dataFinalizacao < dataInicioExecucao))
                throw new DomainException("A data de finalização não pode ser anterior à data de início de execução.", ErrorType.DomainRuleBroken);

            if (dataEntrega.HasValue && (!dataFinalizacao.HasValue || dataEntrega < dataFinalizacao))
                throw new DomainException("A data de entrega não pode ser anterior à data de finalização.", ErrorType.DomainRuleBroken);

            _dataCriacao = dataCriacao;
            _dataInicioExecucao = dataInicioExecucao;
            _dataFinalizacao = dataFinalizacao;
            _dataEntrega = dataEntrega;
        }

        public DateTime DataCriacao => _dataCriacao;
        public DateTime? DataInicioExecucao => _dataInicioExecucao;
        public DateTime? DataFinalizacao => _dataFinalizacao;
        public DateTime? DataEntrega => _dataEntrega;

        /// <summary>
        /// Reidrata o HistoricoTemporal a partir de dados do banco SEM VALIDAÇÃO de ordem cronológica.
        /// NÃO deve ser usado fora do contexto de buscar do banco.
        /// </summary>
        public static HistoricoTemporal Reidratar(DateTime dataCriacao, DateTime? dataInicioExecucao, DateTime? dataFinalizacao, DateTime? dataEntrega)
        {
            return new HistoricoTemporal
            {
                _dataCriacao = dataCriacao,
                _dataInicioExecucao = dataInicioExecucao,
                _dataFinalizacao = dataFinalizacao,
                _dataEntrega = dataEntrega
            };
        }

        public HistoricoTemporal MarcarDataInicioExecucao(DateTime? data = null)
            => new(_dataCriacao, data ?? DateTime.UtcNow, _dataFinalizacao, _dataEntrega);

        public HistoricoTemporal MarcarDataFinalizadaExecucao(DateTime? data = null)
            => new(_dataCriacao, _dataInicioExecucao, data ?? DateTime.UtcNow, _dataEntrega);

        public HistoricoTemporal MarcarDataEntrega(DateTime? data = null)
            => new(_dataCriacao, _dataInicioExecucao, _dataFinalizacao, data ?? DateTime.UtcNow);
    }
}

