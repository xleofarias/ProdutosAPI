using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.Models
{
    public class Produtos
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}
