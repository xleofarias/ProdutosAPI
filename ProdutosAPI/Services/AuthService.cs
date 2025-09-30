using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ProdutosAPI.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace ProdutosAPI.Services
{
    // Serviço de autenticação
    public class AuthService : IAuthService
    {
        private readonly string _jwtKey;

        public AuthService(IConfiguration configuration)
        {
            _jwtKey = configuration["Jwt:Key"];
        }
        public string GenerateToken(Usuarios usuario)
        {
            //Instancia do manipulador de tokens
            var tokenHandler = new JwtSecurityTokenHandler();

            //Chave secreta para assinatura do token
            var key = Encoding.ASCII.GetBytes(_jwtKey);

            //Criação dos claims
            var claims = RoleClaimExtension.GetClaims(usuario);

            var TokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                //Claims que irão compor o token
                Subject = new System.Security.Claims.ClaimsIdentity(claims), 
                //Tempo de expiração do token
                Expires = DateTime.UtcNow.AddHours(2), 
                //Assinatura do token, serve para identificar que mandou o token e garantir que o token não foi alterado no meio do caminho.
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) 
            };

            //Criação do token
            var token = tokenHandler.CreateToken(TokenDescriptor);

            //Retorna o token em formato string
            return tokenHandler.WriteToken(token);
        }
    }
}
