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

        /// <summary>
        /// Buscar o usuário
        /// </summary>
        /// <param name="id">Código do Usuário</param>
        /// <response code="200">Usuário retornardo com sucesso</response>
        /// <response code="401">Não Autorizado</response>
        /// <response code="404">Usuário não encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Dados do usuário com base no código passado: <paramref name="id"/></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetByIdAsync(int id)
        {
            var user = await _usuarioService.GetAsync(x => x.Id == id);

            return Ok(user);
        }


        /// <summary>
        /// Cadastrar o usuário
        /// </summary>
        /// <response code="201">Usuário cadastrado com sucesso</response>
        /// <response code="401">Não Autorizado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Retorna os dados do usuário cadastrado</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> PostAsync([FromBody] UserRequestDto usuario)
        {
            var user = await _usuarioService.CreateAsync(usuario);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = user.Id }, usuario);
        }


        /// <summary>
        /// Alterar função do usuário
        /// </summary>
        /// <param name="id">Código do Usuário</param>
        /// <response code="200">Função alterada com sucesso</response>
        /// <response code="401">Não Autorizado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Função alterada</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch]
        public async Task<IActionResult> PatchChangeUserRole(int userId, int newRoleId)
        {
            await _usuarioService.UpdateRoleAsync(userId, newRoleId);

            return Ok("Função alterada com sucesso");
        }
    }
}
