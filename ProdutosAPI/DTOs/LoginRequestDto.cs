using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProdutosAPI.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "O Email deve ser válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "A senha é obrigatória")]
        //[PasswordPropertyText(true)]
        public string Senha { get; set; }
    }
}
