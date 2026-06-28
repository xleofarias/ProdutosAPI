using ProdutosAPI.Models;
using System.Linq.Expressions;
using ProdutosAPI.DTOs;

namespace ProdutosAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken ct = default);
        Task<User> CreateAsync(User user, CancellationToken ct = default);
        Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken ct = default);
        Task<bool> UpdateRoleAsync(int userId, int newRoleId, CancellationToken ct = default);
        
    }
}