using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetAsync(Expression<Func<User, bool>> predicate);
        Task<UserResponseDto> CreateAsync(UserRequestDto user);
        Task<bool> UpdateRoleAsync(int userId, int newRoleId);
    }
}
