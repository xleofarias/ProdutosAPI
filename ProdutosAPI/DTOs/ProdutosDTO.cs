using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.DTOs
{
    // DTO (Data Transfer Object) para representar os dados dos produtos
    public class ProdutosDTO
    {
        [Required(ErrorMessage ="O nome do produto é obrigatório.")]
        public string Nome { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "O Preço do produto não deve ser negativo")]
        public decimal Preco { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser um número não negativo.")]
        public int Quantidade { get; set; }
    }
}
