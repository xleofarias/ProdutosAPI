namespace Contracts.Events
{
    public record UserCreatedEvent(int UserId, string Name, DateTime CreatedAt);
}
