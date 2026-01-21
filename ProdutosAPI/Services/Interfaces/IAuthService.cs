using ProdutosAPI.Models;

namespace ProdutosAPI.Services.Interfaces
{
    // Interface para o serviço de autenticação
    public interface IAuthService
    {
        string GenerateToken(User user);
    }
}
