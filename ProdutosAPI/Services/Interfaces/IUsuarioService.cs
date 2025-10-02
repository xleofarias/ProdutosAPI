using ProdutosAPI.DTOs;
using ProdutosAPI.Models;

namespace ProdutosAPI.Services.Interfaces
{
    public interface IUsuarioService
    {
        public Task<Usuarios> PostRegistrar(RegisterUserDTO registerUser);

        public Task<Usuarios> GetUsuarioByLogin(LoginDTO login);

        public Task<Usuarios> PatchChangeUserRole(int userId, int newRoleId);
    }
}
