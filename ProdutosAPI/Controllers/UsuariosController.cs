using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using SecureIdentity.Password;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Datas;
using ProdutosAPI.Extensions;


namespace ProdutosAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [AllowAnonymous]
        [HttpPost("registrar")]
        public async Task<ActionResult<Usuarios>> PostRegistrar([FromBody] RegisterUserDTO usuario, [FromServices] ProdutosDBContext dbContext)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _usuarioService.PostRegistrar(usuario);

            return CreatedAtAction(nameof(GetUsuariosById), new { id = user.Id }, user);
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
                var token = new Token().GenerateToken(usuario);
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
