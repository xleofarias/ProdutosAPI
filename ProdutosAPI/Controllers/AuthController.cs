using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.Datas;
using ProdutosAPI.DTOs;
using ProdutosAPI.Services.Interfaces;
using SecureIdentity.Password;


namespace ProdutosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _auth;
        public AuthController(AppDbContext context, IAuthService auth)
        {
            _context = context;
            _auth = auth;
        }

        [AllowAnonymous]
        [HttpPost]  
        public async Task<ActionResult<string>> PostToken([FromBody] LoginDTO login)
        {
            var usuario = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == login.Email);

            if (usuario is null) throw new KeyNotFoundException("Usuário não encontrado");

            if (!PasswordHasher.Verify(usuario.PasswordHash, login.Senha))
            {
                throw new ArgumentException("Senha inválida");
            }

            var token = _auth.GenerateToken(usuario);
            return Ok(token);
        }
    }
}
