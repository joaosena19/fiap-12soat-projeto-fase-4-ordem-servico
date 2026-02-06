namespace Infrastructure.Database
{
    /// <summary>
    /// Stub temporário para SeedData.
    /// As funções de seed de outros bounded contexts foram removidas.
    /// Apenas OrdemServico é mantido neste bounded context.
    /// </summary>
    public static class SeedData
    {
        // Stubs para manter compatibilidade com testes antigos que serão removidos no A-09
        public static void SeedClientes(AppDbContext context) { }
        public static void SeedVeiculos(AppDbContext context) { }
        public static void SeedServicos(AppDbContext context) { }
        public static void SeedItensEstoque(AppDbContext context) { }
        public static void SeedUsuarios(AppDbContext context) { }
        public static void SeedRoles(AppDbContext context) { }
        public static void SeedOrdensServico(AppDbContext context) { }
        public static void SeedAll(AppDbContext context) { }
    }
}
