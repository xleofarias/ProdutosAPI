namespace ProdutosAPI.Events
{
    public record UserCreatedEvent(int UserId, string Name, DateTime CreatedAt);
}
