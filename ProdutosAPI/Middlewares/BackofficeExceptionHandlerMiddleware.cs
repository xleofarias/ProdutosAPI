using System.Net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ProdutosAPI.Middlewares
{
    public class BackofficeExceptionHandlerMiddleware : AbstractExceptionHandlerMiddleware
    {
        public BackofficeExceptionHandlerMiddleware(RequestDelegate next) : base(next)
        {
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
                    break;
                case ArgumentException
                     or DbUpdateException
                     or ArgumentNullException:
                    code = HttpStatusCode.BadRequest;
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    break;
            }
            return (code, JsonConvert.SerializeObject(ex.Message));
        }
    }
}
