using Reqnroll;
using Tests.BDD.Support.OrdemServico;
using Tests.Integration;
using Tests.Integration.Fixtures;
using Tests.Integration.Helpers;

namespace Tests.BDD.Hooks
{
    [Binding]
    public class BDDHooks
    {
        private readonly CriarOrdemServicoCompletaContexto _contexto;

        public BDDHooks(CriarOrdemServicoCompletaContexto contexto)
        {
            _contexto = contexto;
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            _contexto.MongoFixture = new Mongo2GoFixture();
            await _contexto.MongoFixture.InitializeAsync();

            _contexto.Factory = new TestWebApplicationFactory(_contexto.MongoFixture);

            await MongoOrdemServicoTestHelper.ClearAsync(_contexto.Factory.Services);
            _contexto.Factory.Mocks.ResetAll();
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            if (_contexto.Factory != null)
            {
                await MongoOrdemServicoTestHelper.ClearAsync(_contexto.Factory.Services);
                _contexto.Client?.Dispose();
                _contexto.Factory?.Dispose();
            }

            if (_contexto.MongoFixture != null)
                await _contexto.MongoFixture.DisposeAsync();
        }
    }
}
