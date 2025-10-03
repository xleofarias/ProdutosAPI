using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Datas;
using ProdutosAPI.DTOs;
using ProdutosAPI.Extensions;
using ProdutosAPI.Models;
using ProdutosAPI.Services.Interfaces;
using SecureIdentity.Password;
using System.Linq.Expressions;

namespace ProdutosAPI.Services
{
    public class UsuariosService : IUsuarioService
    {
        private readonly ProdutosDBContext _context;
        public UsuariosService(ProdutosDBContext context)
        {
            _context = context;
        }

        public async Task<Usuarios> GetUsuariosById(Expression<Func<Usuarios, bool>> predicate)
        {
            var user = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(predicate);

            if (user is null) throw new KeyNotFoundException("Usuário não encontrado");

            return user;
        }

        public async Task<bool> PatchChangeUserRole(int userId, int newRoleId)
        {
            var user = await _context.Usuarios.FindAsync(userId);

            if (user is null) throw new KeyNotFoundException("Usuário não encontrado");

            var role = await _context.Roles.FindAsync(newRoleId);

            if (role is null) throw new KeyNotFoundException("Função não encontrada");

            try
            {
                user.Role = role;
                await _context.SaveChangesAsync();

                return true;
            }
            catch 
            {
                return false;
            }
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

            if (await _context.Usuarios.AnyAsync(x => x.Slug == user.Slug))
            {
                throw new DbUpdateException("E-mail já cadastrado");
            }

            if (await _context.Usuarios.AnyAsync(x => x.Login == user.Login))
            {
                throw new DbUpdateException("Login já cadastrado");
            }

            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();
            
            return user;
        }
    }
}