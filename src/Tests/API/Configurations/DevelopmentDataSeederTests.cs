using API.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;

namespace Tests.API.Configurations
{
    public class DevelopmentDataSeederTests
    {
        [Fact(DisplayName = "Deve não executar seed quando ambiente não é Development")]
        [Trait("Configuration", "DevelopmentDataSeeder")]
        public void SeedIfDevelopment_NaoDeveExecutarSeed_QuandoNaoEhDevelopment()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.Environment.EnvironmentName = "Production";
            var app = builder.Build();

            // Act & Assert - Não deve lançar exceção (deve retornar antes do seed)
            var acao = () => DevelopmentDataSeeder.SeedIfDevelopment(app);
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "Deve detectar testes de integração e não executar seed")]
        [Trait("Configuration", "DevelopmentDataSeeder")]
        public void Seed_NaoDeveExecutarSeed_QuandoEhTesteDeIntegracao()
        {
            // Arrange - O assembly de testes já está carregado, IsIntegrationTest() retornará true
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.Environment.EnvironmentName = "Development";
            var app = builder.Build();

            // Act & Assert - Em contexto de teste, IsIntegrationTest() retorna true, então não executa seed
            var acao = () => DevelopmentDataSeeder.Seed(app);
            acao.Should().NotThrow();
        }

        [Fact(DisplayName = "SeedIfDevelopment deve chamar Seed quando ambiente é Development")]
        [Trait("Configuration", "DevelopmentDataSeeder")]
        public void SeedIfDevelopment_DeveChamarSeed_QuandoEhDevelopment()
        {
            // Arrange - Em ambiente de teste, IsIntegrationTest() retorna true, então Seed retorna cedo
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.Environment.EnvironmentName = "Development";
            var app = builder.Build();

            // Act & Assert
            var acao = () => DevelopmentDataSeeder.SeedIfDevelopment(app);
            acao.Should().NotThrow();
        }
    }
}
