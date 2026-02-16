using Application.OrdemServico.Dtos;
using System.Net.Http;
using Tests.Integration;
using Tests.Integration.Fixtures;

namespace Tests.BDD.Support.OrdemServico
{
    public class CriarOrdemServicoCompletaContexto
    {
        public Mongo2GoFixture MongoFixture { get; set; } = null!;
        public TestWebApplicationFactory Factory { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;
        public CriarOrdemServicoCompletaDto Request { get; set; } = null!;
        public HttpResponseMessage Response { get; set; } = null!;
        public Guid? OrdemServicoIdCriada { get; set; }
        public Guid ClienteIdExistente { get; set; }
        public Guid VeiculoIdExistente { get; set; }
        public Guid ServicoIdExistente { get; set; }
        public Guid ServicoIdInexistente { get; set; }
        public Guid ItemEstoqueIdExistente { get; set; }
        public Guid ItemEstoqueIdInexistente { get; set; }
    }
}
