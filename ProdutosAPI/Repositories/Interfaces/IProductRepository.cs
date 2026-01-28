using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        public Task<Product> GetByFindAsync(Expression<Func<Product, bool>> predicate);
        public Task<IEnumerable<Product>> GetAllAsync();
        public Task<Product> CreateAsync(Product product);
        public Task<bool> UpdateAsync(int id, Product product);
        public Task<bool> DeleteAsync(int id);
    }
}