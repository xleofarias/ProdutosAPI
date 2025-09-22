using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.DTOs
{
    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; }
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "O Email deve ser válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Senha { get; set; }
    }
}
