using ProdutosAPI.Models;

namespace ProdutosAPI.Services.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(Usuarios usuario);
    }
}
