using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.Models
{
    public class Produtos
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public required string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}
