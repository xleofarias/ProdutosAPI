using Moq;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.DTOs;
using System.Linq.Expressions;
using ProdutosAPI.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ProdutosAPITests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<ILogger<ProductService>>_logger;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockCache = new Mock<IDistributedCache>();
            _logger = new Mock<ILogger<ProductService>>();
            _service = new ProductService(_mockRepo.Object, _mockCache.Object, _logger.Object);
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenProductExists() 
        {
            // Arrange - Preparação
            var product = new Product {Id = 1, Name = "Nescau", Price = 3.0m, Quantity = 1};

            // Serializa para Byte[] (como o Redis guarda de verdade)
            var json = JsonSerializer.Serialize(product);
            var bytes = Encoding.UTF8.GetBytes(json);

            //Simula que foi no bancno e retornou o produto
            _mockCache.Setup(r => r.GetAsync(
                "List_Products",
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            _mockRepo.Setup(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(product);

            //Act - Ação/
            var produto = await _service.GetByFindAsync(p => p.Id == 1);

            //Assert - Verificação
            Assert.NotNull(produto);

            // Confirmar se o método foi chamado
            _mockRepo.Verify(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenProductDoesNotExist() 
        {
            //Configurar o simulado
            _mockRepo.Setup(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync((Product?)null);

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByFindAsync(p => p.Id == 99));

            // Confirmar se o método foi chamado
            _mockRepo.Verify(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnFromCache_WhenCacheExists()
        {
            //Configurar o que dever se simulado
            var productsFake = new List<Product> {
                new Product { Id = 1, Name = "Nescau", Price = 2.0m, Quantity = 1 },
                new Product { Id = 2, Name = "Arroz", Price = 2.5m, Quantity = 2 }
            };

            // Serializa para Byte[] (como o Redis guarda de verdade)
            var json = JsonSerializer.Serialize(productsFake);
            var bytes = Encoding.UTF8.GetBytes(json);

            _mockCache.Setup(c => c.GetAsync(
                    "List_Products",
                    It.IsAny<CancellationToken>())).ReturnsAsync(bytes);

            var result = await _service.GetAllAsync();

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Nescau", result.First().Name);

            _mockRepo.Verify(r => r.GetAllAsync(), Times.Never);
        }

        //[Fact]
        //public async Task GetProdutos_DeveLancarExcessao_QuandoNaoExistir() 
        //{
        //    _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        //    //Act - Ação & Assert - Verificação
        //    await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync());

        //    // Confirmar se o método foi chamado
        //    _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        //}

        [Fact]
        public async Task CreateAsync_ShouldReturnDto_WhenProductIsValid()
        {
            //Configurar o que dever se simulado
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                .ReturnsAsync(new Product { Id = 1, Name = "Nescau", Quantity= 10, Price= 5.50m });

            var requestDto = new ProductDTO { Name = "Nescau", Quantity = 10, Price = 5.50m };

            //Act - Ação
            var produto = await _service.CreateAsync(requestDto);

            // Assert - Verificação
            Assert.NotNull(produto);
            Assert.Equal("Nescau", produto.Name);

            // Confirmar se o método foi chamado
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
        }

        //[Fact]
        //public async Task PostProdutos_DeveLancarExcessao_QuandoNaoCriar() 
        //{
        //    // Arrange - Preparação
        //    var mockRepo = new Mock<IProductRepository>();

        //    mockRepo.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync((Product)null);

        //    var service = new ProductService(mockRepo.Object);
        //    var requestDto = new ProductDTO { Name = "Nescau", Quantity = 10, Price = 5.50m };

        //    //Act - Ação & Assert - Verificação
        //    //await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(requestDto));

        //    // Confirmar se o método foi chamado
        //    mockRepo.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
        //}

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenNameAlreadyExists() 
        {
            // Arrange - Preparação
            var productExisting  = new Product{ Name = "Nescau", Price = 4.0m, Quantity =2};

            _mockRepo.Setup(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(productExisting);

            var requestDTO = new ProductDTO { Name = "Nescau", Price = 5.0m, Quantity =3};
            
            //Act - Ação
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(requestDTO));

            Assert.Equal("Já existe um produto com esse nome cadastrado", ex.Message);

            _mockRepo.Verify(r => r.GetByFindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);

            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Never);
        }
    }
}
