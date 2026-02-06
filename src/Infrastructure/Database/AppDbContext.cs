using Domain.OrdemServico.Aggregates.OrdemServico;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<OrdemServico>  OrdensServico { get; set; }
        public DbSet<ServicoIncluido> ServicosIncluidos { get; set; }
        public DbSet<ItemIncluido> ItensIncluidos { get; set; }
        public DbSet<Orcamento> Orcamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
