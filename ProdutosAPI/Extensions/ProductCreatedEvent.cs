namespace ProdutosAPI.Extensions
{
    public record ProductCreatedEvent(int ProductId, string Name, DateTime CreatedAt)
    {

    }
}
