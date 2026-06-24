using ProdutosAPI.Models;
using System.Linq.Expressions;
using ProdutosAPI.DTOs;

namespace ProdutosAPI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByFindAsync(Expression<Func<Product, bool>> predicate, CancellationToken ct = default);
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default);
        Task<Product> CreateAsync(Product product, CancellationToken ct = default);
        Task<PagedResult<Product>> ProductPaginationDtoAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, Product product, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}