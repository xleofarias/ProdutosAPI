using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProdutosAPI.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Login { get; set; }
        public required string PasswordHash { get; set; }   
        public required string Slug { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
