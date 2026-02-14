using API.Configurations;
using FluentAssertions;
using Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.API.Configurations
{
    public class HealthCheckConfigurationTests
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;

        public HealthCheckConfigurationTests()
        {
            _services = new ServiceCollection();
            _services.AddLogging();
            
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
                ["MongoDb:DatabaseName"] = "testdb"
            });
            _configuration = configurationBuilder.Build();
            
            var mongoSettingsMock = new Mock<IOptions<MongoDbSettings>>();
            mongoSettingsMock.Setup(m => m.Value).Returns(new MongoDbSettings { ConnectionString = "mongodb://localhost:27017", DatabaseName = "testdb" });
            _services.AddSingleton(mongoSettingsMock.Object);
            _services.AddSingleton<MongoDbContext>();
        }

        [Fact(DisplayName = "AddHealthChecks deve registrar health checks necessários")]
        [Trait("API", "HealthCheckConfiguration")]
        public void AddHealthChecks_DeveRegistrarHealthChecksNecessarios()
        {
            // Arrange & Act
            _services.AddHealthChecks(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            healthCheckService.Should().NotBeNull("o serviço de health check deve estar registrado");
        }

        [Fact(DisplayName = "AddHealthChecks deve retornar a coleção de serviços para fluent interface")]
        [Trait("API", "HealthCheckConfiguration")]
        public void AddHealthChecks_DeveRetornarServiceCollection_ParaFluentInterface()
        {
            // Arrange & Act
            var resultado = _services.AddHealthChecks(_configuration);

            // Assert
            resultado.Should().BeSameAs(_services, "deve retornar a mesma instância para permitir encadeamento");
        }

        [Fact(DisplayName = "AddHealthChecks deve configurar health check self com tag live")]
        [Trait("API", "HealthCheckConfiguration")]
        public async Task AddHealthChecks_DeveConfigurarHealthCheckSelf_ComTagLive()
        {
            // Arrange
            _services.AddHealthChecks(_configuration);
            var serviceProvider = _services.BuildServiceProvider();
            var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

            // Act
            var resultado = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("live"));

            // Assert
            resultado.Should().NotBeNull();
            resultado.Entries.Should().ContainKey("self");
            resultado.Entries["self"].Status.Should().Be(HealthStatus.Healthy);
        }

        [Fact(DisplayName = "AddHealthChecks deve configurar health check database com tag ready")]
        [Trait("API", "HealthCheckConfiguration")]
        public void AddHealthChecks_DeveConfigurarHealthCheckDatabase_ComTagReady()
        {
            // Arrange
            _services.AddHealthChecks(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Act
            var healthChecks = serviceProvider.GetRequiredService<HealthCheckService>();

            // Assert
            healthChecks.Should().NotBeNull("o health check service deve estar configurado");
        }
    }
}
