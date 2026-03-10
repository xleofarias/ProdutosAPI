using ProdutosAPI.DTOs;

namespace ProdutosAPI.Services.Interfaces
{
    // Interface para o serviço de autenticação
    public interface IAuthService
    {
        Task<LoginReponseDto> LoginAsync(LoginRequestDto login);
    }
}
