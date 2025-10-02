using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Datas;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using SecureIdentity.Password;

namespace ProdutosAPI.Services
{
    public class UsuariosService : IUsuarioService
    {
        private readonly ProdutosDBContext _context;
        public UsuariosService(ProdutosDBContext context)
        {
            _context = context;
        }
        public Task<Usuarios> GetUsuarioByLogin(LoginDTO login)
        {
            throw new NotImplementedException();
        }

        public Task<Usuarios> PatchChangeUserRole(int userId, int newRoleId)
        {
            throw new NotImplementedException();
        }

        public async Task<Usuarios> PostRegistrar(RegisterUserDTO registerUser)
        {
            var user = new Usuarios
            {
                Nome = registerUser.Nome,
                Login = registerUser.Login,
                Email = registerUser.Email,
                Slug = registerUser.Email.Replace("@", "-").Replace(".", "-"),
                RoleId = 1,
                PasswordHash = PasswordHasher.Hash(registerUser.Senha)
            };

            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
