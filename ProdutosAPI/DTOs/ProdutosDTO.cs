namespace ProdutosAPI.DTOs
{
    public class ProdutosDTO
    {
        public required string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}
