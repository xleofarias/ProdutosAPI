using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ProdutosAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Product> GetByFindAsync(Expression<Func<Product, bool>> predicate)
        {
            return await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbContext.Products.AsNoTracking().ToListAsync();
        }
        public async Task<Product> CreateAsync(Product product)
        {
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }
        public async Task<bool> UpdateAsync(int id, Product product)
        {
            // Usando ExecuteUpdateAsync para atualizar diretamente no banco de dadosS
            return await _dbContext.Products
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.Name, product.Name)
                    .SetProperty(p => p.Price, product.Price)
                    .SetProperty(p => p.Quantity, product.Quantity)
                ) > 0;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            // Usando ExecuteDeleteAsync para deletar diretamente no banco de dados
            return await _dbContext.Products
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync() > 0;
        }
    }
}
