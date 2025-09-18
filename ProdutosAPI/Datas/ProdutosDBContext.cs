using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Models;

namespace ProdutosAPI.Datas
{
    public class ProdutosDBContext : DbContext
    {
        public ProdutosDBContext(DbContextOptions<ProdutosDBContext> options) : base(options)
        {
        }
        public DbSet<Produtos> Produtos { get; set; }
    }
}
