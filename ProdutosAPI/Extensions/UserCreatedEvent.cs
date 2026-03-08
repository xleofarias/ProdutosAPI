namespace ProdutosAPI.Extensions
{
    public record UserCreatedEvent(int UserId, string Name, DateTime CreatedAt);
}
