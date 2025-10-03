using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using ProdutosAPI.Datas;


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


        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuarios>> GetUsuariosById(int id)
        {
            var user = await _usuarioService.GetUsuariosById(x => x.Id == id);

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Usuarios>> PostRegistrar([FromBody] RegisterUserDTO usuario, [FromServices] ProdutosDBContext dbContext)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _usuarioService.PostRegistrar(usuario);

            return CreatedAtAction(nameof(GetUsuariosById), new { id = user.Id }, usuario);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch]
        public async Task<IActionResult> PatchChangeUserRole(int userId, int newRoleId)
        {
            await _usuarioService.PatchChangeUserRole(userId, newRoleId);

            return Ok("Função alterada com sucesso");
        }
    }
}
