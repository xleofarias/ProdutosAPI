using ProdutosAPI.Models;

namespace ProdutosAPITests.DTOsTests
{
    public record UserDToTest()
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public int RoleId { get; init; }
        public Role? Role { get; init; }
    }
}