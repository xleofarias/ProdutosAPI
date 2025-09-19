using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Models;

namespace ProdutosAPI.Datas
{
    public class ProdutosDBContext : DbContext
    {
        public ProdutosDBContext(DbContextOptions<ProdutosDBContext> options) : base(options)
        {
        }
        // Define a propriedade DbSet para a entidade Produtos
        public DbSet<Produtos> Produtos { get; set; }
    }
}
