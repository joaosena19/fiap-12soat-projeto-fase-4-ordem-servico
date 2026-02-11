using Domain.OrdemServico.Aggregates.OrdemServico;
using Domain.OrdemServico.Enums;
using FluentAssertions;
using Infrastructure.Database;
using MongoDB.Driver;
using Moq;
using Shared.Seed;

namespace Tests.Other.DataSeed
{
    public class SeedDataMongoTests
    {
        [Fact(DisplayName = "Deve popular 3 ordens de serviço quando coleção estiver vazia")]
        [Trait("Componente", "Seed")]
        [Trait("Método", "SeedOrdensServicoAsync")]
        public async Task SeedOrdensServicoAsync_Deve_PopularOrdensServico_Quando_ColecaoVazia()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<OrdemServico>>();
            var capturedOrdens = new List<OrdemServico>();

            mockCollection
                .Setup(c => c.EstimatedDocumentCountAsync(default, default))
                .ReturnsAsync(0);

            mockCollection
                .Setup(c => c.InsertManyAsync(It.IsAny<IEnumerable<OrdemServico>>(), null, default))
                .Callback<IEnumerable<OrdemServico>, InsertManyOptions, CancellationToken>((ordens, opts, ct) =>
                {
                    capturedOrdens.AddRange(ordens);
                })
                .Returns(Task.CompletedTask);

            // Act
            await SeedData.SeedOrdensServicoAsync(mockCollection.Object);

            // Assert
            capturedOrdens.Should().HaveCount(3);

            // Verifica cenário 1: Cancelada
            var cancelada = capturedOrdens.FirstOrDefault(o => o.VeiculoId == SeedIds.Veiculos.Abc1234);
            cancelada.Should().NotBeNull();
            cancelada!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
            cancelada.ItensIncluidos.Should().HaveCount(2);

            // Verifica cenário 2: Entregue
            var entregue = capturedOrdens.FirstOrDefault(o => o.VeiculoId == SeedIds.Veiculos.Xyz5678);
            entregue.Should().NotBeNull();
            entregue!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Entregue);
            entregue.ServicosIncluidos.Should().HaveCount(2);

            // Verifica cenário 3: EmDiagnostico
            var emDiagnostico = capturedOrdens.FirstOrDefault(o => o.VeiculoId == SeedIds.Veiculos.Def9012);
            emDiagnostico.Should().NotBeNull();
            emDiagnostico!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmDiagnostico);
            emDiagnostico.ServicosIncluidos.Should().BeEmpty();
            emDiagnostico.ItensIncluidos.Should().BeEmpty();
        }

        [Fact(DisplayName = "Não deve popular ordens de serviço quando coleção já tiver dados")]
        [Trait("Componente", "Seed")]
        [Trait("Método", "SeedOrdensServicoAsync")]
        public async Task SeedOrdensServicoAsync_NaoDevePopular_Quando_ColecaoJaTiverDados()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<OrdemServico>>();

            mockCollection
                .Setup(c => c.EstimatedDocumentCountAsync(default, default))
                .ReturnsAsync(5); // Coleção não está vazia

            // Act
            await SeedData.SeedOrdensServicoAsync(mockCollection.Object);

            // Assert
            mockCollection.Verify(
                c => c.InsertManyAsync(It.IsAny<IEnumerable<OrdemServico>>(), null, default),
                Times.Never,
                "InsertManyAsync não deve ser chamado quando a coleção já tem dados");
        }

        [Fact(DisplayName = "Deve utilizar IDs determinísticos do SeedIds")]
        [Trait("Componente", "Seed")]
        [Trait("Método", "SeedOrdensServicoAsync")]
        public async Task SeedOrdensServicoAsync_Deve_UtilizarIdsDeterministicos()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<OrdemServico>>();
            var capturedOrdens = new List<OrdemServico>();

            mockCollection
                .Setup(c => c.EstimatedDocumentCountAsync(default, default))
                .ReturnsAsync(0);

            mockCollection
                .Setup(c => c.InsertManyAsync(It.IsAny<IEnumerable<OrdemServico>>(), null, default))
                .Callback<IEnumerable<OrdemServico>, InsertManyOptions, CancellationToken>((ordens, opts, ct) =>
                {
                    capturedOrdens.AddRange(ordens);
                })
                .Returns(Task.CompletedTask);

            // Act
            await SeedData.SeedOrdensServicoAsync(mockCollection.Object);

            // Assert - Verifica que os IDs de veículos são os esperados
            capturedOrdens.Select(o => o.VeiculoId).Should().Contain(SeedIds.Veiculos.Abc1234);
            capturedOrdens.Select(o => o.VeiculoId).Should().Contain(SeedIds.Veiculos.Xyz5678);
            capturedOrdens.Select(o => o.VeiculoId).Should().Contain(SeedIds.Veiculos.Def9012);

            // Verifica que os IDs de itens/serviços também são determinísticos
            var ordemComItens = capturedOrdens.First(o => o.VeiculoId == SeedIds.Veiculos.Abc1234);
            ordemComItens.ItensIncluidos.Select(i => i.ItemEstoqueOriginalId).Should().Contain(SeedIds.ItensEstoque.OleoMotor5w30);
            ordemComItens.ItensIncluidos.Select(i => i.ItemEstoqueOriginalId).Should().Contain(SeedIds.ItensEstoque.FiltroDeOleo);

            var ordemComServicos = capturedOrdens.First(o => o.VeiculoId == SeedIds.Veiculos.Xyz5678);
            ordemComServicos.ServicosIncluidos.Select(s => s.ServicoOriginalId).Should().Contain(SeedIds.Servicos.TrocaDeOleo);
            ordemComServicos.ServicosIncluidos.Select(s => s.ServicoOriginalId).Should().Contain(SeedIds.Servicos.AlinhamentoBalanceamento);
        }

        [Fact(DisplayName = "SeedAllAsync deve chamar SeedOrdensServicoAsync")]
        [Trait("Componente", "Seed")]
        [Trait("Método", "SeedAllAsync")]
        public async Task SeedAllAsync_Deve_ChamarSeedOrdensServicoAsync()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<OrdemServico>>();
            var mockContext = new Mock<MongoDbContext>("mongodb://localhost:27017", "testdb");

            mockCollection
                .Setup(c => c.EstimatedDocumentCountAsync(default, default))
                .ReturnsAsync(0);

            mockCollection
                .Setup(c => c.InsertManyAsync(It.IsAny<IEnumerable<OrdemServico>>(), null, default))
                .Returns(Task.CompletedTask);

            mockContext
                .Setup(c => c.OrdensServico)
                .Returns(mockCollection.Object);

            // Act
            await SeedData.SeedAllAsync(mockContext.Object);

            // Assert
            mockCollection.Verify(
                c => c.EstimatedDocumentCountAsync(default, default),
                Times.Once,
                "EstimatedDocumentCountAsync deve ser chamado para verificar idempotência");
        }
    }
}
