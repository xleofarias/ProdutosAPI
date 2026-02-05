using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ProdutosAPI.Extensions;
using Microsoft.IdentityModel.Tokens;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.DTOs;
using BCrypt.Net;

namespace ProdutosAPI.Services
{
    // Serviço de autenticação
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtKey;

        public AuthService(IUserRepository userRepository,IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtKey = configuration["Jwt:Key"];
        }
        public async Task<LoginReponseDto> LoginAsync(LoginRequestDto login)
        {
            // Busca o usuário pelo email
            var user = await _userRepository.GetByEmailWithRoleAsync(login.Email);

            if (user == null)
            {
                throw new Exception("Usuário não encontrado");
            };

            // 👇 O X-9 (DEBUG): O que DIABOS tem nesse banco?
            Console.WriteLine($"[DEBUG] Hash vindo do banco: '{user.PasswordHash}'");
            Console.WriteLine($"[DEBUG] Tamanho do Hash: {user.PasswordHash?.Length}");
            Console.WriteLine($"[DEBUG] Senha digitada: '{login.Senha}'");

            // Verifica se a senha está correta
            if (!BCrypt.Net.BCrypt.Verify(user.PasswordHash, login.Senha))
            {
                throw new Exception("Senha inválida");
            };

            var token = GenerateToken(user);

            return new LoginReponseDto
            {
                Email = user.Email,
                Token = token
            };
        }

        private string GenerateToken(User user)
        {
            //Instancia do manipulador de tokens
            var tokenHandler = new JwtSecurityTokenHandler();

            //Chave secreta para assinatura do token
            var key = Encoding.ASCII.GetBytes(_jwtKey);

            //Criação dos claims
            var claims = RoleClaimExtension.GetClaims(user);

            var TokenDescriptor = new SecurityTokenDescriptor
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
