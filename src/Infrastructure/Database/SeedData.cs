using MongoDB.Driver;
using Domain.OrdemServico.Aggregates.OrdemServico;
using Domain.OrdemServico.Enums;
using Shared.Seed;
using Infrastructure.Repositories.OrdemServico;

namespace Infrastructure.Database
{
    /// <summary>
    /// Responsável por popular a base de dados com dados iniciais.
    /// Implementa padrão idempotente - executa apenas se a coleção estiver vazia.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Seed de ordens_servico collection.
        /// </summary>
        public static async Task SeedOrdensServicoAsync(IMongoCollection<OrdemServicoDocument> collection, CancellationToken ct = default)
        {
            // Verifica idempotência - executa seed apenas se a coleção estiver vazia
            var count = await collection.EstimatedDocumentCountAsync(cancellationToken: ct);
            if (count > 0)
                return;

            var ordensServico = new List<OrdemServico>();

            // Cenário 1: Cancelada (Orçamento desaprovado)
            var ordem1 = OrdemServico.Criar(SeedIds.Veiculos.Abc1234);
            ordem1.IniciarDiagnostico();
            ordem1.AdicionarItem(SeedIds.ItensEstoque.OleoMotor5w30, "Óleo Motor 5W30", 50.00m, 2, TipoItemIncluidoEnum.Peca);
            ordem1.AdicionarItem(SeedIds.ItensEstoque.FiltroDeOleo, "Filtro de Óleo", 30.00m, 1, TipoItemIncluidoEnum.Peca);
            ordem1.GerarOrcamento();
            ordem1.DesaprovarOrcamento();
            ordensServico.Add(ordem1);

            // Cenário 2: Entregue (Fluxo completo)
            var ordem2 = OrdemServico.Criar(SeedIds.Veiculos.Xyz5678);
            ordem2.IniciarDiagnostico();
            ordem2.AdicionarServico(SeedIds.Servicos.TrocaDeOleo, "Troca de Óleo", 80.00m);
            ordem2.AdicionarServico(SeedIds.Servicos.AlinhamentoBalanceamento, "Alinhamento e Balanceamento", 120.00m);
            ordem2.GerarOrcamento();
            ordem2.AprovarOrcamento();
            ordem2.IniciarExecucao();
            ordem2.FinalizarExecucao();
            ordem2.Entregar();
            ordensServico.Add(ordem2);

            // Cenário 3: EmDiagnostico (Início do fluxo)
            var ordem3 = OrdemServico.Criar(SeedIds.Veiculos.Def9012);
            ordem3.IniciarDiagnostico();
            ordensServico.Add(ordem3);

            // Converte aggregates para documents e insere
            var documents = ordensServico.Select(OrdemServicoMapper.ToDocument).ToList();
            await collection.InsertManyAsync(documents, cancellationToken: ct);
        }

        /// <summary>
        /// Seeds all collections in the database.
        /// </summary>
        public static async Task SeedAllAsync(MongoDbContext context, CancellationToken ct = default)
        {
            await SeedOrdensServicoAsync(context.OrdensServico, ct);
        }
    }
}
