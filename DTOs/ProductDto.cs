using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.DTOs
{
    // DTO (Data Transfer Object) para representar os dados dos produtos
    public class ProductDTO
    {
        [Required(ErrorMessage = "The Name field is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "The Name must be between 3 and 100 characters.")]
        public string Name { get; set; }
        // Dica: Range com double.MaxValue em decimal as vezes dá warning. 
        // Para preço, geralmente validamos se é maior que 0.
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }
    }
}
