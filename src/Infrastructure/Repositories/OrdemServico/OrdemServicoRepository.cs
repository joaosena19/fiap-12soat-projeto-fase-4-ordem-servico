using Application.Contracts.Gateways;
using Domain.OrdemServico.Enums;
using Infrastructure.Database;
using MongoDB.Driver;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Infrastructure.Repositories.OrdemServico
{
    public class OrdemServicoRepository : IOrdemServicoGateway
    {
        private readonly IMongoCollection<OrdemServicoAggregate> _collection;

        public OrdemServicoRepository(MongoDbContext context)
        {
            _collection = context.OrdensServico;
        }

        public async Task<OrdemServicoAggregate> SalvarAsync(OrdemServicoAggregate ordemServico)
        {
            await _collection.InsertOneAsync(ordemServico);
            return ordemServico;
        }

        public async Task<OrdemServicoAggregate?> ObterPorIdAsync(Guid id)
        {
            return await _collection
                .Find(os => os.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<OrdemServicoAggregate?> ObterPorCodigoAsync(string codigo)
        {
            var codigoNormalizado = codigo.Trim().Replace("-", "").ToUpper();
            
            var filter = Builders<OrdemServicoAggregate>.Filter.Where(os => 
                os.Codigo.Valor.Trim().Replace("-", "").ToUpper() == codigoNormalizado);
            
            return await _collection
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task<OrdemServicoAggregate> AtualizarAsync(OrdemServicoAggregate ordemServico)
        {
            // MongoDB replace - substitui o documento inteiro
            var result = await _collection.ReplaceOneAsync(
                os => os.Id == ordemServico.Id,
                ordemServico,
                new ReplaceOptions { IsUpsert = false });

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Ordem de Serviço com Id {ordemServico.Id} não encontrada para atualização");

            return ordemServico;
        }

        public async Task<IEnumerable<OrdemServicoAggregate>> ObterTodosAsync()
        {
            return await _collection
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdemServicoAggregate>> ObterEntreguesUltimosDiasAsync(int quantidadeDias)
        {
            var dataLimite = DateTime.UtcNow.AddDays(-quantidadeDias).Date;
            
            var filter = Builders<OrdemServicoAggregate>.Filter.And(
                Builders<OrdemServicoAggregate>.Filter.Eq(os => os.Status.Valor, StatusOrdemServicoEnum.Entregue),
                Builders<OrdemServicoAggregate>.Filter.Gte(os => os.Historico.DataCriacao, dataLimite)
            );
            
            return await _collection
                .Find(filter)
                .ToListAsync();
        }
    }
}
