using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using SecureIdentity.Password;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Datas;


namespace ProdutosAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [AllowAnonymous]
        [HttpPost("registrar")]
        public async Task<IActionResult> PostRegistrar([FromBody] RegisterUserDTO usuario, [FromServices] ProdutosDBContext dbContext)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new Usuarios
            {
                Nome = usuario.Nome,
                Login = usuario.Login,
                Email = usuario.Email,
                Slug = usuario.Email.Replace("@", "-").Replace(".", "-"),
                RoleId = 1,
                PasswordHash = PasswordHasher.Hash(usuario.Senha)
            };

            try
            {
                await dbContext.Usuarios.AddAsync(user);
                await dbContext.SaveChangesAsync();
                return Ok($"{user.Email} {user.PasswordHash}");
            }
            catch (DbUpdateException e)
            {
                return BadRequest(new { message = "Não foi possível registrar o usuário", error = e.Message });

            }
            catch
            {
                return StatusCode(500, "Erro interno no servidor");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> PostLogin([FromBody] LoginDTO user, [FromServices] ProdutosDBContext dbContext)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await dbContext.Usuarios
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == user.Email);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            if (!PasswordHasher.Verify(usuario.PasswordHash, user.Senha))
            {
                return BadRequest(new { message = "Senha inválida" });
            }

            try
            {
                var token = _auth.GenerateToken(usuario);
                return Ok(new { token });
            }
            catch
            {
                return StatusCode(500, "Erro interno no servidor");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPatch]
        public async Task<IActionResult> PatchChangeUserRole(int userId, int newRoleId, [FromServices] ProdutosDBContext context)
        {
            var user = await context.Usuarios.FindAsync(userId);
            
            if(user is null) return BadRequest("Usuário não encontrado");

            var role = await context.Roles.FindAsync(newRoleId);

            if(role is null) return BadRequest("Função não encontrada");

            user.Role = role;

            await context.SaveChangesAsync();

            return Ok("Função alterada com sucesso");
        }
    }
}
