using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Repositories
{
    public class UserRepository(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public async Task<User> CreateAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);

            return user;
        }

        public async Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken ct = default)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate, ct);
        }

        public async Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<bool> UpdateRoleAsync(int userId, int newRoleId, CancellationToken ct = default)
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, ct);

            if (user is null) return false;

            user.RoleId = newRoleId;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
