using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Tests.Helpers;
using Tests.Integration.Fixtures;
using Tests.Integration.Mocks;
using Application.Contracts.Monitoramento;
using Application.OrdemServico.Interfaces.External;
using Microsoft.Extensions.Options;

namespace Tests.Integration
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncDisposable
        where TStartup : class
    {
        private readonly MongoDbFixture _mongoFixture;
        private readonly string _databaseName;
        private readonly MockExternalServices _mockExternalServices;

        public TestWebApplicationFactory()
        {
            // Gera uma database única para cada classe de teste
            var stackTrace = new StackTrace();
            var callingFrame = stackTrace.GetFrames()
                ?.FirstOrDefault(f => f.GetMethod()?.DeclaringType?.Name?.EndsWith("Tests") == true);
            
            var testClassName = callingFrame?.GetMethod()?.DeclaringType?.Name ?? "Unknown";
            _databaseName = $"TestDb_{testClassName}_{Guid.NewGuid():N}";

            _mongoFixture = new MongoDbFixture();
            _mockExternalServices = new MockExternalServices();
        }

        public MockExternalServices MockExternalServices => _mockExternalServices;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Inicializar MongoDB fixture
            _mongoFixture.InitializeAsync().GetAwaiter().GetResult();

            // Sobrescreve o appsettings para usar configurações de teste
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var overrides = new Dictionary<string, string?>
                {
                    ["Webhook:HmacSecret"] = TestHmacUtils.TestHmacSecret,
                    ["Jwt:Key"] = JwtTestConstants.Key,
                    ["Jwt:Issuer"] = JwtTestConstants.Issuer,
                    ["Jwt:Audience"] = JwtTestConstants.Audience,
                    ["Logging:LogLevel:Default"] = "None",
                    ["Logging:LogLevel:Microsoft"] = "None",
                    ["Logging:LogLevel:Microsoft.AspNetCore"] = "None",
                    ["MongoDB:ConnectionString"] = _mongoFixture.ConnectionString,
                    ["MongoDB:DatabaseName"] = _mongoFixture.DatabaseName
                };
                config.AddInMemoryCollection(overrides);
            });

            builder.ConfigureServices(services =>
            {
                // Configurar mocks para os serviços externos
                services.AddSingleton<IClienteExternalService>(_mockExternalServices.ClienteService);
                services.AddSingleton<IVeiculoExternalService>(_mockExternalServices.VeiculoService);
                services.AddSingleton<IServicoExternalService>(_mockExternalServices.ServicoService);
                services.AddSingleton<IEstoqueExternalService>(_mockExternalServices.EstoqueService);

                // Configurar mock do IMetricsService para testes
                services.AddSingleton<IMetricsService, MockMetricsService>();

                // Reconfigura JWT com valores de teste
                services.Configure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                    Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes(JwtTestConstants.Key)),
                            ValidateIssuer = true,
                            ValidIssuer = JwtTestConstants.Issuer,
                            ValidateAudience = true,
                            ValidAudience = JwtTestConstants.Audience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    });
            });

            base.ConfigureWebHost(builder);
        }
        
        // Cleanup método para limpar dados entre testes
        public async Task ClearDatabaseAsync()
        {
            var context = _mongoFixture.CreateContext();
            await context.OrdensServico.DeleteManyAsync(MongoDB.Driver.Builders<global::Domain.OrdemServico.Aggregates.OrdemServico.OrdemServico>.Filter.Empty);
            _mockExternalServices.Clear();
        }

        // IAsyncDisposable para limpar recursos
        public new async ValueTask DisposeAsync()
        {
            await _mongoFixture.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
