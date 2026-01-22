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
        public static IEnumerable<Claim> GetClaims(this User user)
        {
            // Cria uma lista de claims com as informações do usuário
            var result = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               // new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
              //  new Claim(ClaimTypes.Role, user.Role.Nome)
            };

            // Adiciona a claim de função se o usuário tiver uma função associada
            if (user.Role != null)
            {
                result.Add(new Claim(ClaimTypes.Role, user.Role.Nome));
            }

            return result;
        }
    }
}
