using Microsoft.IdentityModel.Tokens;
using ProdutosAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ProdutosAPI.Extensions
{
    public class Token
    {
        private readonly string _jwtKey;

        public Token(IConfiguration configuration)
        {
            _jwtKey = configuration["Jwt:Key"];
        }

        public Token() { }

        public string GenerateToken(User usuario)
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
