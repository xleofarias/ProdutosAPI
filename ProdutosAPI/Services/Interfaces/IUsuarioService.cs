using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Services.Interfaces
{
    public interface IUsuarioService
    {
        public Task<Usuarios> GetUsuariosById(Expression<Func<Usuarios, bool>> predicate);
        public Task<Usuarios> PostRegistrar(RegisterUserDTO registerUser);
        public Task<bool> PatchChangeUserRole(int userId, int newRoleId);
    }
}
