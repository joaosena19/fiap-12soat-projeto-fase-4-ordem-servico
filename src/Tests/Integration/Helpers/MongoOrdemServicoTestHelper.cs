using Infrastructure.Database;
using Infrastructure.Repositories.OrdemServico;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OrdemServicoAggregate = global::Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico;

namespace Tests.Integration.Helpers;

/// <summary>
/// Helper para operações comuns de MongoDB nos testes de integração
/// </summary>
public static class MongoOrdemServicoTestHelper
{
    /// <summary>
    /// Limpa todos os documentos da coleção ordens_servico
    /// </summary>
    public static async Task ClearAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<MongoDbContext>();
        await context.OrdensServico.DeleteManyAsync(FilterDefinition<OrdemServicoDocument>.Empty);
    }

    /// <summary>
    /// Insere uma ordem de serviço diretamente no MongoDB (seed)
    /// </summary>
    public static async Task InsertSeedAsync(IServiceProvider services, OrdemServicoAggregate ordemServico)
    {
        var context = services.GetRequiredService<MongoDbContext>();
        var document = OrdemServicoMapper.ToDocument(ordemServico);
        await context.OrdensServico.InsertOneAsync(document);
    }

    /// <summary>
    /// Busca ordem de serviço por Id
    /// </summary>
    public static async Task<OrdemServicoAggregate?> FindByIdAsync(IServiceProvider services, Guid id)
    {
        var context = services.GetRequiredService<MongoDbContext>();
        var filter = Builders<OrdemServicoDocument>.Filter.Eq(os => os.Id, id);
        var document = await context.OrdensServico.Find(filter).FirstOrDefaultAsync();
        return document != null ? OrdemServicoMapper.ToAggregate(document) : null;
    }

    /// <summary>
    /// Busca ordem de serviço por Codigo
    /// </summary>
    public static async Task<OrdemServicoAggregate?> FindByCodigoAsync(IServiceProvider services, string codigo)
    {
        var context = services.GetRequiredService<MongoDbContext>();
        // Busca por código usando propriedade plana do document
        var filter = Builders<OrdemServicoDocument>.Filter.Eq(os => os.Codigo, codigo);
        var document = await context.OrdensServico.Find(filter).FirstOrDefaultAsync();
        return document != null ? OrdemServicoMapper.ToAggregate(document) : null;
    }
}
