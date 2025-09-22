using Microsoft.Identity.Client;
using ProdutosAPI.Models;
using System.Security.Claims;

namespace ProdutosAPI.Extensions
{
    /// <summary>
    /// Extensão para obter claims de um usuário.
    /// </summary>
    public static class RoleClaimExtension
    {
        public static IEnumerable<Claim> GetClaims(this Usuarios usuario)
        {
            // Cria uma lista de claims com as informações do usuário
            var result = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role.Nome)
            };

            return result;
        }
    }
}
