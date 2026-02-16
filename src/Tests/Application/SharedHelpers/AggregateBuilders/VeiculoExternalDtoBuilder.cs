using Application.OrdemServico.Dtos.External;
using Bogus;

namespace Tests.Application.SharedHelpers.AggregateBuilders
{
    public class VeiculoExternalDtoBuilder
    {
        private Guid _clienteId = Guid.NewGuid();
        private string _placa;
        private string _modelo;
        private string _marca;
        private string _cor;
        private int _ano;
        private string _tipoVeiculo;
        private readonly Faker _faker = new Faker("pt_BR");

        public VeiculoExternalDtoBuilder()
        {
            _placa = _faker.Random.Replace("???-####");
            _modelo = _faker.Vehicle.Model();
            _marca = _faker.Vehicle.Manufacturer();
            _cor = _faker.Commerce.Color();
            _ano = _faker.Random.Int(2000, DateTime.Now.Year);
            _tipoVeiculo = _faker.PickRandom("Carro", "Moto");
        }

        public VeiculoExternalDtoBuilder ComClienteId(Guid clienteId)
        {
            _clienteId = clienteId;
            return this;
        }

        public VeiculoExternalDtoBuilder ComPlaca(string placa)
        {
            _placa = placa;
            return this;
        }

        public VeiculoExternalDtoBuilder ComModelo(string modelo)
        {
            _modelo = modelo;
            return this;
        }

        public VeiculoExternalDtoBuilder ComMarca(string marca)
        {
            _marca = marca;
            return this;
        }

        public VeiculoExternalDtoBuilder ComCor(string cor)
        {
            _cor = cor;
            return this;
        }

        public VeiculoExternalDtoBuilder ComAno(int ano)
        {
            _ano = ano;
            return this;
        }

        public VeiculoExternalDtoBuilder ComTipoVeiculo(string tipoVeiculo)
        {
            _tipoVeiculo = tipoVeiculo;
            return this;
        }

        public VeiculoExternalDto Build()
        {
            return new VeiculoExternalDto
            {
                ClienteId = _clienteId,
                Placa = _placa,
                Modelo = _modelo,
                Marca = _marca,
                Cor = _cor,
                Ano = _ano,
                TipoVeiculo = _tipoVeiculo
            };
        }
    }
}
