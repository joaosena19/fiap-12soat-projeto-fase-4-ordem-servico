using Application.OrdemServico.Dtos.External;
using Bogus;
using Domain.OrdemServico.Enums;

namespace Tests.Application.OrdemServico.Helpers
{
    public class ItemEstoqueExternalDtoBuilder
    {
        private Guid _id;
        private string _nome;
        private decimal _preco;
        private int _quantidade;
        private string _tipoItemEstoque;
        private readonly Faker _faker = new Faker("pt_BR");

        public ItemEstoqueExternalDtoBuilder()
        {
            _id = Guid.NewGuid();
            _nome = _faker.Commerce.ProductName();
            _preco = _faker.Random.Decimal(10, 500);
            _quantidade = _faker.Random.Int(1, 100);
            _tipoItemEstoque = _faker.PickRandom<TipoItemIncluidoEnum>().ToString();
        }

        public ItemEstoqueExternalDtoBuilder ComId(Guid id)
        {
            _id = id;
            return this;
        }

        public ItemEstoqueExternalDtoBuilder ComNome(string nome)
        {
            _nome = nome;
            return this;
        }

        public ItemEstoqueExternalDtoBuilder ComPreco(decimal preco)
        {
            _preco = preco;
            return this;
        }

        public ItemEstoqueExternalDtoBuilder ComQuantidade(int quantidade)
        {
            _quantidade = quantidade;
            return this;
        }

        public ItemEstoqueExternalDtoBuilder ComTipoItemEstoque(TipoItemIncluidoEnum tipoItemEstoque)
        {
            _tipoItemEstoque = tipoItemEstoque.ToString();
            return this;
        }

        public ItemEstoqueExternalDtoBuilder ComTipoItemEstoque(string tipoItemEstoque)
        {
            _tipoItemEstoque = tipoItemEstoque;
            return this;
        }

        public ItemEstoqueExternalDto Build()
        {
            return new ItemEstoqueExternalDto
            {
                Id = _id,
                Nome = _nome,
                Preco = _preco,
                Quantidade = _quantidade,
                TipoItemEstoque = _tipoItemEstoque
            };
        }
    }
}