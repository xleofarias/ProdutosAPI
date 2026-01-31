using Microsoft.AspNetCore.Diagnostics; // Necessário para IExceptionHandlerFeature
using Microsoft.AspNetCore.Mvc;         // Necessário para ControllerBase e IActionResult

namespace ProdutosAPI.Controllers
{
    [ApiController]
    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [HttpPatch]
        public IActionResult HandleError()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return Problem(
                detail: "Ocorreu um erro interno. Tente novamente mais tarde.",
                title: "Erro Interno",
                statusCode: 500
            );
        }
    }
}
