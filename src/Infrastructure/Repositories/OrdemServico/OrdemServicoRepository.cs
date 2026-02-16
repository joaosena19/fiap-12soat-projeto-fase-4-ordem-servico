using Application.Contracts.Gateways;
using Domain.OrdemServico.Enums;
using Infrastructure.Database;
using MongoDB.Driver;
using OrdemServicoAggregate = Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Infrastructure.Repositories.OrdemServico
{
    public class OrdemServicoRepository : IOrdemServicoGateway
    {
        private readonly IMongoCollection<OrdemServicoDocument> _collection;

        public OrdemServicoRepository(MongoDbContext context)
        {
            _collection = context.OrdensServico;
        }

        public async Task<OrdemServicoAggregate> SalvarAsync(OrdemServicoAggregate ordemServico)
        {
            var document = OrdemServicoMapper.ToDocument(ordemServico);
            await _collection.InsertOneAsync(document);
            return ordemServico;
        }

        public async Task<OrdemServicoAggregate?> ObterPorIdAsync(Guid id)
        {
            var document = await _collection
                .Find(os => os.Id == id)
                .FirstOrDefaultAsync();
            
            return document != null ? OrdemServicoMapper.ToAggregate(document) : null;
        }

        public async Task<OrdemServicoAggregate?> ObterPorCodigoAsync(string codigo)
        {
            var codigoTrimmed = codigo.Trim().ToUpper();
            
            // Se o código não tem 18 caracteres (formato OS-YYYYMMDD-XXXXXX), tenta formatar
            if (codigoTrimmed.Length != 18)
            {
                var codigoSemHifens = codigoTrimmed.Replace("-", "");
                
                if (codigoSemHifens.Length == 16 && codigoSemHifens.StartsWith("OS"))
                    codigoTrimmed = $"OS-{codigoSemHifens.Substring(2, 8)}-{codigoSemHifens.Substring(10)}";
            }
            
            var document = await _collection
                .Find(os => os.Codigo == codigoTrimmed)
                .FirstOrDefaultAsync();
            
            return document != null ? OrdemServicoMapper.ToAggregate(document) : null;
        }

        public async Task<OrdemServicoAggregate> AtualizarAsync(OrdemServicoAggregate ordemServico)
        {
            var document = OrdemServicoMapper.ToDocument(ordemServico);
            
            var result = await _collection.ReplaceOneAsync(
                os => os.Id == ordemServico.Id,
                document,
                new ReplaceOptions { IsUpsert = false });

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Ordem de Serviço com Id {ordemServico.Id} não encontrada para atualização");

            return ordemServico;
        }

        public async Task<IEnumerable<OrdemServicoAggregate>> ObterTodosAsync()
        {
            var documents = await _collection
                .Find(_ => true)
                .ToListAsync();
            
            return documents.Select(OrdemServicoMapper.ToAggregate);
        }

        public async Task<IEnumerable<OrdemServicoAggregate>> ObterEntreguesUltimosDiasAsync(int quantidadeDias)
        {
            var dataLimite = DateTime.UtcNow.AddDays(-quantidadeDias).Date;
            
            var filter = Builders<OrdemServicoDocument>.Filter.And(
                Builders<OrdemServicoDocument>.Filter.Eq(os => os.Status, StatusOrdemServicoEnum.Entregue.ToString()),
                Builders<OrdemServicoDocument>.Filter.Gte("Historico.DataCriacao", dataLimite)
            );
            
            var documents = await _collection
                .Find(filter)
                .ToListAsync();
            
            return documents.Select(OrdemServicoMapper.ToAggregate);
        }

        public async Task<IEnumerable<OrdemServicoAggregate>> ObterOrdensAguardandoEstoqueComTimeoutAsync(DateTime timeoutLimit)
        {
            var filter = Builders<OrdemServicoDocument>.Filter.And(
                Builders<OrdemServicoDocument>.Filter.Eq(os => os.Status, StatusOrdemServicoEnum.EmExecucao.ToString()),
                Builders<OrdemServicoDocument>.Filter.Eq("InteracaoEstoque.DeveRemoverEstoque", true),
                Builders<OrdemServicoDocument>.Filter.Eq("InteracaoEstoque.EstoqueRemovidoComSucesso", (bool?)null),
                Builders<OrdemServicoDocument>.Filter.Lte("Historico.DataInicioExecucao", timeoutLimit)
            );

            var documents = await _collection.Find(filter).ToListAsync();
            
            return documents.Select(OrdemServicoMapper.ToAggregate);
        }
    }
}
