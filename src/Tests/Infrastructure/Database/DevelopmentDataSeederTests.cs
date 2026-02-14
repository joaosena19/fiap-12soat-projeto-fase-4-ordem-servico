using API.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Tests.Infrastructure.Database;

[Trait("Infrastructure", "Database")]
public class DevelopmentDataSeederTests
{
    [Fact(DisplayName = "SeedIfDevelopment não deve lançar exceção quando ambiente não for Development")]
    public void SeedIfDevelopment_NaoDeveLancarExcecao_QuandoAmbienteNaoForDevelopment()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production
        });
        
        using var app = builder.Build();

        // Act & Assert
        FluentActions.Invoking(() => DevelopmentDataSeeder.SeedIfDevelopment(app))
            .Should().NotThrow();
    }

    [Fact(DisplayName = "Seed não deve lançar exceção quando estiver em ambiente de testes")]
    public void Seed_NaoDeveLancarExcecao_QuandoEstiverEmAmbienteDeTestes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        
        using var app = builder.Build();

        // Act & Assert
        FluentActions.Invoking(() => DevelopmentDataSeeder.Seed(app))
            .Should().NotThrow("porque IsIntegrationTest() deve retornar true e o método deve retornar cedo");
    }
}
