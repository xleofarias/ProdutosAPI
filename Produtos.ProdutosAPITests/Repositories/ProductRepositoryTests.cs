using ProdutosAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Repositories;
using ProdutosAPI.Data;

namespace ProdutosAPITests.Repositories
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;

        public ProductRepositoryTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public async Task ProductPaginationKeyAsync_ShouldReturnNextPage_WhenMoreProductsExist(){

            //Arrange
            var ct = CancellationToken.None;

            var products = new List<Product>{
                new Product{Id = 1, Name = "Nescau", Price = 3.0m, Quantity = 1},
                new Product{Id = 2, Name = "Nescau1", Price = 3.0m, Quantity = 1},
                new Product{Id = 3, Name = "Nescau2", Price = 3.0m, Quantity = 1},
                new Product{Id = 4, Name = "Nescau3", Price = 3.0m, Quantity = 1},
                new Product{Id = 5, Name = "Nescau4", Price = 3.0m, Quantity = 1}
            };

            _context.Products.AddRange(products);

            await _context.SaveChangesAsync(ct);

            var repository = new ProductRepository(_context);

            //Act
            var result = await repository.ProductPaginationKeyAsync(2,2, ct);

            //Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(3, result.Items[0].Id);
            Assert.Equal(4, result.Items[1].Id);
            Assert.Equal(4, result.NextCursor);
            Assert.True(result.HasNextPage);
        }

        [Fact]
        public async Task ProductPaginationKeyAsync_ShouldReturnNoNextPage_WhenLastPageIsReached(){
            
            //Arrange
            var ct = CancellationToken.None;

            var products = new List<Product>{
                new Product{Id = 1, Name = "Nescau", Price = 3.0m, Quantity = 1},
                new Product{Id = 2, Name = "Nescau1", Price = 3.0m, Quantity = 1},
                new Product{Id = 3, Name = "Nescau2", Price = 3.0m, Quantity = 1},
                new Product{Id = 4, Name = "Nescau3", Price = 3.0m, Quantity = 1},
                new Product{Id = 5, Name = "Nescau4", Price = 3.0m, Quantity = 1}
            }; 

            _context.Products.AddRange(products);

            await _context.SaveChangesAsync(ct);

            var repository = new ProductRepository(_context);

            //Act
            var result = await repository.ProductPaginationKeyAsync(4,2, ct);

            //Assert
            Assert.Single(result.Items);
            Assert.Equal(5, result.Items[0].Id);
            Assert.Null(result.NextCursor);
            Assert.False(result.HasNextPage);
        }

    }
}