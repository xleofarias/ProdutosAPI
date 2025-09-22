using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Registrar([FromBody] RegisterUserDTO usuario)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //var user = new Usuarios
            //{
            //    Nome = usuario.Nome,
            //    Email = usuario.Email,
            //    PasswordHash = PasswordHasher.Hash(usuario.Senha),
            //};
            // Lógica para registrar o usuário (salvar no banco de dados, etc.)
            // ...
            return Ok("Usuário registrado com sucesso!");
        }
    }
}
