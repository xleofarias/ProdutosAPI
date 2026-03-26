using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Services.Interfaces
{
    // Interface para o serviço de produtos
    public interface IProductService
    {
       public  Task<Product> GetByFindAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellation = default);
       public  Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellation = default);
       public  Task<Product> CreateAsync(ProductDTO product, CancellationToken cancellation = default);
       public  Task<bool> UpdateAsync(int id, ProductDTO produto, CancellationToken cancellation = default);
       public  Task<bool> DeleteAsync(int id, CancellationToken cancellation = default);
    }
}
