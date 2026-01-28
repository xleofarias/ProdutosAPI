using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.DTOs
{
    public record UserRequestDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; init; } // 'init' mantém a imutabilidade do record

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; init; }

        [Required]
        public string Login { get; init; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must have at least 6 characters")]
        public string Password { get; init; }
        [Required]
        public int RoleId { get; init; }
    }
}