using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;

namespace ProdutosAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _jwtKey;

        public AuthService(IConfiguration configuration)
        {
            _jwtKey = configuration["Jwt:Key"];
        }
        public string GenerateToken(Usuarios usuario)
        {
            throw new NotImplementedException();
        }
    }
}
