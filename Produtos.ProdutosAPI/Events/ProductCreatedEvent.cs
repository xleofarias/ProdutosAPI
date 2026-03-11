namespace ProdutosAPI.Events
{
    public record ProductCreatedEvent(int ProductId, string Name, DateTime CreatedAt);
}
