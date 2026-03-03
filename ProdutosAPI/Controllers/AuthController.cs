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
        
        /// <summary>
        /// Authenticates a user and returns a login response containing a token.
        /// </summary>
        /// <response code="200">Ok</response>
        /// <response code="500">Error Internal Server</response>
        /// <param name="login">The login request data including user credentials.</param>
        /// <returns>An action result containing the login response.</returns>
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
