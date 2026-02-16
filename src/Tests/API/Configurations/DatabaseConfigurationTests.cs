using API.Configurations;
using FluentAssertions;
using Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.API.Configurations
{
    public class DatabaseConfigurationTests
    {
        [Fact(DisplayName = "Deve lançar exceção quando ConnectionString não está configurada")]
        [Trait("Configuration", "DatabaseConfiguration")]
        public void AddDatabase_DeveLancarExcecao_QuandoConnectionStringNaoConfigurada()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "MongoDB:DatabaseName", "testdb" }
                })
                .Build();

            var services = new ServiceCollection();

            // Act & Assert
            FluentActions.Invoking(() => DatabaseConfiguration.AddDatabase(services, configuration))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("*ConnectionString*");
        }

        [Fact(DisplayName = "Deve lançar exceção quando DatabaseName não está configurado")]
        [Trait("Configuration", "DatabaseConfiguration")]
        public void AddDatabase_DeveLancarExcecao_QuandoDatabaseNameNaoConfigurado()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "MongoDB:ConnectionString", "mongodb://localhost:27017" }
                })
                .Build();

            var services = new ServiceCollection();

            // Act & Assert
            FluentActions.Invoking(() => DatabaseConfiguration.AddDatabase(services, configuration))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("*DatabaseName*");
        }

        [Fact(DisplayName = "Deve configurar serviços quando configuração válida")]
        [Trait("Configuration", "DatabaseConfiguration")]
        public void AddDatabase_DeveConfigurarServicos_QuandoConfiguracaoValida()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "MongoDB:ConnectionString", "mongodb://localhost:27017" },
                    { "MongoDB:DatabaseName", "testdb" }
                })
                .Build();

            var services = new ServiceCollection();

            // Act
            var resultado = DatabaseConfiguration.AddDatabase(services, configuration);

            // Assert
            resultado.Should().NotBeNull();
            services.Should().Contain(sd => sd.ServiceType == typeof(MongoDbContext));
        }

        [Fact(DisplayName = "Deve lançar exceção quando ConnectionString é vazia")]
        [Trait("Configuration", "DatabaseConfiguration")]
        public void AddDatabase_DeveLancarExcecao_QuandoConnectionStringVazia()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "MongoDB:ConnectionString", "" },
                    { "MongoDB:DatabaseName", "testdb" }
                })
                .Build();

            var services = new ServiceCollection();

            // Act & Assert
            FluentActions.Invoking(() => DatabaseConfiguration.AddDatabase(services, configuration))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("*ConnectionString*");
        }

        [Fact(DisplayName = "Deve lançar exceção quando DatabaseName é vazio")]
        [Trait("Configuration", "DatabaseConfiguration")]
        public void AddDatabase_DeveLancarExcecao_QuandoDatabaseNameVazio()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "MongoDB:ConnectionString", "mongodb://localhost:27017" },
                    { "MongoDB:DatabaseName", "" }
                })
                .Build();

            var services = new ServiceCollection();

            // Act & Assert
            FluentActions.Invoking(() => DatabaseConfiguration.AddDatabase(services, configuration))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("*DatabaseName*");
        }
    }
}
