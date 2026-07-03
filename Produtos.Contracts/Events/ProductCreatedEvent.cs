namespace Contracts.Events
{
    public record class ProductCreatedEvent(int ProductId, string Name, decimal Price, DateTime CreatedAt);
}
