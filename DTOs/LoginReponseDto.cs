using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.DTOs
{
    public class LoginReponseDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Token { get; set; }
    }
}
