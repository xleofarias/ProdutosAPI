using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosAPI.DTOs;
using ProdutosAPI.Services.Interfaces;


namespace ProdutosAPI.Controllers
{
    [Authorize]
   // [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _usuarioService;

        public UserController(IUserService usuarioService)
        {
            _usuarioService = usuarioService;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetByIdAsync(int id)
        {
            var user = await _usuarioService.GetAsync(x => x.Id == id);

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> PostAsync([FromBody] UserRequestDto usuario)
        {
            var user = await _usuarioService.CreateAsync(usuario);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = user.Id }, usuario);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch]
        public async Task<IActionResult> PatchChangeUserRole(int userId, int newRoleId)
        {
            await _usuarioService.UpdateRoleAsync(userId, newRoleId);

            return Ok("Função alterada com sucesso");
        }
    }
}
