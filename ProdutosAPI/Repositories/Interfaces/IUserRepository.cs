using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(Expression<Func<User, bool>> predicate);
        Task<User> CreateAsync(User user);
        Task<bool> UpdateRoleAsync(int userId, int newRoleId);
    }
}
