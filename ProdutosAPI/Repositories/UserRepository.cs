using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ProdutosAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> UpdateRoleAsync(int userId, int newRoleId)
        {
            var rowAffectd = await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(user => user.RoleId, newRoleId));

            return rowAffectd > 0;
        }
    }
}
