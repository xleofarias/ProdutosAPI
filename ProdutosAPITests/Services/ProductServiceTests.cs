using Moq;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.DTOs;
using System.Linq.Expressions;
using ProdutosAPI.Services;

namespace ProdutosAPITests.Services
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenProductExists() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProductRepository>();
            var product = new Product {Id = 1, Name = "Nescau", Price = 3.0m, Quantity = 1};

            //Simula que foi no bancno e retornou o produto
            mockRepo.Setup(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(product);

            var service = new ProductService(mockRepo.Object);

            //Act - Ação
            var produto = await service.GetByFindAsync(p => p.Id == 1);

            //Assert - Verificação
            Assert.NotNull(produto);

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenProductDoesNotExist() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProductRepository>();

            //Configurar o simulado
            mockRepo.Setup(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync((Product?)null);

            // Obtém o objeto simulado
            var service = new ProductService(mockRepo.Object);

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByFindAsync(p => p.Id == 99));

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetProdutos_DeveRetornarListaDeProdutos_QuandoExistir() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProductRepository>();

            //Configurar o que dever se simulado
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product> 
            {
                new Product {Id = 1, Name = "Nescau", Price = 2.0m, Quantity =1},
                new Product {Id = 2, Name = "Arroz", Price = 2.5m, Quantity= 2}
            });

            // Obtém o objeto simulado
            var service = new ProductService(mockRepo.Object);

            //Act - Ação
            var produtos = await service.GetAllAsync();

            //Assert - Verificação
            Assert.NotNull(produtos);

            Assert.True(produtos.Any());

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProdutos_DeveLancarExcessao_QuandoNaoExistir() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProductRepository>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

            // Obtém o objeto simulado
            var service = new ProductService(mockRepo.Object);

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAllAsync());

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnDto_WhenProductIsValid()
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IProductRepository>();

            //Configurar o que dever se simulado
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                .ReturnsAsync(new Product { Id = 1, Name = "Nescau", Quantity= 10, Price= 5.50m });
        
            var service = new ProductService(mockRepo.Object);
            var requestDto = new ProductDTO { Name = "Nescau", Quantity = 10, Price = 5.50m };

            //Act - Ação
            var produto = await service.CreateAsync(requestDto);

            // Assert - Verificação
            Assert.NotNull(produto);
            Assert.Equal("Nescau", produto.Name);

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task PostProdutos_DeveLancarExcessao_QuandoNaoCriar() 
        {
            // Arrange - Preparação
            var mockRepo = new Mock<IProductRepository>();

            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync((Product)null);

            var service = new ProductService(mockRepo.Object);
            var requestDto = new ProductDTO { Name = "Nescau", Quantity = 10, Price = 5.50m };

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(requestDto));

            // Confirmar se o método foi chamado
            mockRepo.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenNameAlreadyExists() 
        {
            // Arrange - Preparação

            var mockRepo = new Mock<IProductRepository>();
            var productExisting  = new Product{ Name = "Nescau", Price = 4.0m, Quantity =2};

            mockRepo.Setup(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(productExisting);

            var service = new ProductService(mockRepo.Object);
            var requestDTO = new ProductDTO { Name = "Nescau", Price = 5.0m, Quantity =3};
            
            //Act - Ação
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(requestDTO));

            Assert.Equal("Product with the same name already exists.", ex.Message);

            mockRepo.Verify(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);

            mockRepo.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Never);
        }
    }
}
