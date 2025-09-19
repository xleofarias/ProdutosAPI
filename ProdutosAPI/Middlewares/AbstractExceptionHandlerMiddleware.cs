using System.Net;

namespace ProdutosAPI.Middlewares
{
    /// <summary>
    /// Middleware para manipular as exceções.
    /// </summary>
    public abstract class AbstractExceptionHandlerMiddleware
    {
        // Próximo middleware na cadeia
        private readonly RequestDelegate _next;

        // Método abstrato para obter o código de status e a mensagem de resposta
        public abstract (HttpStatusCode code, string message) GetResponse(Exception ex);

        // Construtor
        public AbstractExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Método InvokeAsync
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // get the response code and message
                var (status, message) = GetResponse(ex);
                context.Response.StatusCode = (int)status;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(message);
            }
        }
    }
}
