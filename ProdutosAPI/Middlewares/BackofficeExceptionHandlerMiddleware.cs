using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ProdutosAPI.Extensions;

namespace ProdutosAPI.Middlewares
{
    public class BackofficeExceptionHandlerMiddleware : AbstractExceptionHandlerMiddleware
    {
        private readonly ILogger<BackofficeExceptionHandlerMiddleware> _logger;

        public BackofficeExceptionHandlerMiddleware(RequestDelegate next, ILogger<BackofficeExceptionHandlerMiddleware> logger) : base(next)
        {
            _logger = logger;
        }

        // Implementação do método abstrato GetResponse
        public override (HttpStatusCode code, string message) GetResponse(Exception ex)
        {
            HttpStatusCode code;
            switch (ex)
            {
                case KeyNotFoundException
                     or FileNotFoundException:
                    code = HttpStatusCode.NotFound;
                    _logger.LogWarning(ex.Message, "Recurso não encontrado. Status: {StatusCode}", code);
                    break;
                case ArgumentException
                     or DbUpdateException
                     or ArgumentNullException:
                    code = HttpStatusCode.BadRequest;
                    _logger.LogWarning(ex.Message, "Requisição inválida ou erro de regra de negócio. Status {StatusCode}", code);
                    break;
                case ConflictException:
                    code = HttpStatusCode.Conflict;
                    _logger.LogWarning(ex.Message, "Conflito. Status {StatusCode}", code);
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    _logger.LogCritical(ex.Message, "Erro interno não tratado no servidor!");
                    break;
            }
            return (code, JsonSerializer.Serialize(ex.Message));
        }
    }
}
