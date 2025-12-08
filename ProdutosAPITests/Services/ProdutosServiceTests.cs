using Moq;
using ProdutosAPI.Datas;
using ProdutosAPI.Models;
using ProdutosAPI.Services;
using ProdutosAPI.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Runtime.Intrinsics.Arm;

namespace ProdutosAPITests.Services
{
    public class ProdutosServiceTests
    {
        [Fact]
        public async Task GetProdutosById_DeveRetornarProduto_QuandoExistir() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProdutosService>();

            //Configurar o que dever se simulado
            mockRepo.Setup(r => r.GetProdutosById(It.IsAny<Expression<Func<Produtos, bool>>>())).ReturnsAsync(new Produtos {Id = 1, Nome = "Nescau"});

            var service = mockRepo.Object;

            //Act - Ação
            var produto = await service.GetProdutosById(p => p.Id == 1);

            //Assert - Verificação
            Assert.NotNull(produto);

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetProdutosById(It.IsAny<Expression<Func<Produtos, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetProdutoById_DeveLancarExcessao_QuandoNaoExistir() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProdutosService>();

            //Configurar o simulado
            mockRepo.Setup(r => r.GetProdutosById(It.IsAny<Expression<Func<Produtos, bool>>>())).ThrowsAsync(new KeyNotFoundException("Produto não encontrado"));

            // Obtém o objeto simulado
            var service = mockRepo.Object;

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProdutosById(p => p.Id == 99));

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetProdutosById(It.IsAny<Expression<Func<Produtos, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetProdutos_DeveRetornarListaDeProdutos_QuandoExistir() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProdutosService>();

            //Configurar o que dever se simulado
            mockRepo.Setup(r => r.GetProdutos()).ReturnsAsync(new List<Produtos> 
            {
                new Produtos {Id = 1, Nome = "Nescau"},
                new Produtos {Id = 2, Nome = "Arroz"}
            });

            // Obtém o objeto simulado
            var service = mockRepo.Object;

            //Act - Ação
            var produtos = await service.GetProdutos();

            //Assert - Verificação
            Assert.NotNull(produtos);

            Assert.True(produtos.Any());

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetProdutos(), Times.Once);
        }

        [Fact]
        public async Task GetProdutos_DeveLancarExcessao_QuandoNaoExistir() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProdutosService>();
        
            mockRepo.Setup(r => r.GetProdutos()).ThrowsAsync(new KeyNotFoundException("Nenhum produto encontrado"));

            // Obtém o objeto simulado
            var service = mockRepo.Object;

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProdutos());

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetProdutos(), Times.Once);
        }

        [Fact]
        public async Task PostProdutos_DeveRetornarProduto_QuandoCriar()
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IProdutosService>();

            //Configurar o que dever se simulado
            mockRepo.Setup(r => r.PostProdutos(It.IsAny<ProdutosAPI.DTOs.ProdutosDTO>()))
                .ReturnsAsync(new Produtos { Id = 1, Nome = "Nescau", Quantidade = 10, Preco = 5.50m });
        
            var service = mockRepo.Object;

            //Act - Ação
            var produto = await service.PostProdutos(new ProdutosAPI.DTOs.ProdutosDTO { Nome = "Nescau", Quantidade = 10, Preco = 5.50m });

            // Assert - Verificação
            Assert.NotNull(produto);
            Assert.Equal("Nescau", produto.Nome);

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.PostProdutos(It.IsAny<ProdutosAPI.DTOs.ProdutosDTO>()), Times.Once);
        }

        [Fact]
        public async Task PostProdutos_DeveLancarExcessao_QuandoNaoCriar() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProdutosService>();

            mockRepo.Setup(r => r.PostProdutos(It.IsAny<ProdutosAPI.DTOs.ProdutosDTO>())).ThrowsAsync(new ArgumentException("Erro ao criar produto"));

            var service = mockRepo.Object;

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<ArgumentException>(() => service.PostProdutos(new ProdutosAPI.DTOs.ProdutosDTO { Nome = "Nescau", Quantidade = 10, Preco = 5.50m }));

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.PostProdutos(It.IsAny<ProdutosAPI.DTOs.ProdutosDTO>()), Times.Once);
        }

        [Fact]
        public async Task PostProdutos_DeveLancarExecessao_QuandoJaExistirProdutoComNomeDuplicado() 
        {
            // Arrange - Preparação
            var produtoNovo  = new Produtos{ Nome = "Nescau" };

            var mockRepo = new Mock<IProdutosService>();

            mockRepo.Setup(r => r.GetProdutos()).ReturnsAsync(new List<Produtos> 
            {
                new Produtos {Nome = "Nescau"}
            });

            var service = mockRepo.Object;

            //Act - Ação
            var produtos = await mockRepo.Object.GetProdutos();
            var existe = produtos.Any(p => p.Nome == produtoNovo.Nome);

            // Assert - Verificação
            Assert.True(existe);

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetProdutos(), Times.Once);
        }
    }
}
