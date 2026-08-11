using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ProdutosAPI.Repositories
{
    public class ProductRepository(AppDbContext dbContext) : IProductRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<Product?> GetByFindAsync(Expression<Func<Product, bool>> predicate, CancellationToken ct = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate, ct);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbContext.Products.AsNoTracking().ToListAsync(ct);
        }
        public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
        {
            await _dbContext.Products.AddAsync(product, ct);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task CreateRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
        {
            await _dbContext.Products.AddRangeAsync(products, ct);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<PagedResult<Product>> ProductPaginationDtoAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var query = _dbContext.Products
                .AsNoTracking()
                .OrderBy(p => p.Id);

            var totalItems = await query.CountAsync(ct);

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return new PagedResult<Product>(products, pageNumber, pageSize, totalItems, totalPages);
        }
        
        public async Task<PagedResultKey<Product>> ProductPaginationKeyAsync(int? cursor, int pageSize, CancellationToken ct = default)
        {
            var query = _dbContext.Products
                .AsNoTracking();

            if(cursor.HasValue)
            {
                query = query.Where(p => p.Id > cursor.Value);
            }

            var products = await query
                .OrderBy(p => p.Id)
                .Take(pageSize + 1)
                .ToListAsync(ct);

            var hasNextPage = products.Count > pageSize;

            // Remove o último item se houver uma próxima página, para que possamos retornar apenas os itens da página atual.
            if(hasNextPage)
            {
                products.RemoveAt(products.Count - 1);
            }

            // O operador ^ é usado para acessar elementos a partir do final da lista. products[^1] retorna o último elemento da lista.
            int? nextCursor = hasNextPage && products.Count > 0 ? products[^1].Id : null;

            return new PagedResultKey<Product>(products, nextCursor, hasNextPage);
        }

        public async Task<bool> UpdateAsync(int id, Product product, CancellationToken ct = default)
        {
            var productToUpdate = await _dbContext.Products.FindAsync(new object[] { id }, ct);

            if(productToUpdate is null) return false;

            productToUpdate.Name = product.Name;
            productToUpdate.Price = product.Price;
            productToUpdate.Quantity = product.Quantity;

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existingProduct = await _dbContext.Products.FindAsync(new object[] { id }, ct);

            if (existingProduct is null) return false;

            _dbContext.Products.Remove(existingProduct);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        
        public async Task SeedAsync(int count = 50, CancellationToken ct = default)
        {
            var faker = new Bogus.Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => decimal.Parse(f.Finance.Amount(1, 1000).ToString("0.00")))
                .RuleFor(p => p.Quantity, f => f.Random.Number(1, 100));

            const int batchSize = 5000;

            for(int i = 0; i < count; i += batchSize)
            {
                var batch = faker.Generate(Math.Min(batchSize, count - i));
                await CreateRangeAsync(batch, ct);

                _dbContext.ChangeTracker.Clear();
            }
        }
    }
}
