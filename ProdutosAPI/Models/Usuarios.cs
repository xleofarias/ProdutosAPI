using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProdutosAPI.Models
{
    [Table("Usuarios")]
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Login { get; set; }
        public required string PasswordHash { get; set; }   
        public string Slug { get; set; }
        public int RoleId { get; set; }
        public Roles Role { get; set; }
    }
}
