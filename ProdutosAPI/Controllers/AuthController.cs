using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProdutosAPI.DTOs;
using ProdutosAPI.Services.Interfaces;

namespace ProdutosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService auth)
        {
            _authService = auth;
        }

        [AllowAnonymous]
        [HttpPost]
        [EnableRateLimiting(policyName:"login-limit")]
        public async Task<ActionResult<LoginReponseDto>> PostToken([FromBody] LoginRequestDto login)
        {
            var userLogin = await _authService.LoginAsync(login);

            return Ok(userLogin);
        }
    }
}
