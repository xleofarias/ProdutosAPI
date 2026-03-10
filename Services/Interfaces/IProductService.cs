using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Services.Interfaces
{
    // Interface para o serviço de produtos
    public interface IProductService
    {
       public  Task<Product> GetByFindAsync(Expression<Func<Product, bool>> predicate);
       public  Task<IEnumerable<Product>> GetAllAsync();
       public  Task<Product> CreateAsync(ProductDTO product);
       public  Task<bool> UpdateAsync(int id, ProductDTO produto);
       public  Task<bool> DeleteAsync(int id);
    }
}
