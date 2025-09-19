using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Services
{
    // Interface para o serviço de produtos
    public interface IProdutosService
    {
       public  Task<Produtos> GetProdutosById(Expression<Func<Produtos, bool>> predicate);
       public  Task<IEnumerable<Produtos>> GetProdutos();
       public  Task<Produtos> PostProdutos(ProdutosDTO produto);
       public  Task<bool> PutProdutos(int id, ProdutosDTO produto);
       public  Task<bool> DeleteProdutos(int id);
    }
}
