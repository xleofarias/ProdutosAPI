namespace ProdutosAPI.DTOs
{
    // DTO (Data Transfer Object) para representar os dados dos produtos
    public class ProdutosDTO
    {
        public required string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}
