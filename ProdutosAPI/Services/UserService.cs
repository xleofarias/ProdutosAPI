using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using ProdutosAPI.DTOs;
using ProdutosAPI.Enums;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services.Interfaces;
using SecureIdentity.Password;
using System.Linq.Expressions;
using BCrypt.Net;

namespace ProdutosAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserResponseDto> CreateAsync(UserRequestDto user)
        {
            var existingUser = await _userRepository.GetAsync(u => u.Email == user.Email || u.Login == user.Login);

            if (existingUser != null)
            {
                // Melhoria: Feedback específico
                if (existingUser.Email == user.Email)
                    throw new Exception("Email already registered.");

                if (existingUser.Login == user.Login)
                    throw new Exception("Login already registered.");
            }

            var userNew = new User
            {
                Name = user.Name,
                Email = user.Email,
                Login = user.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Slug = user.Name.ToLower().Replace(" ", "-"),
                RoleId = user.RoleId
            };

            var roleName = ((ERole)userNew.RoleId).ToString();

            try
            {
                await _userRepository.CreateAsync(userNew);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error registering user: {ex.Message}");
            }

            return new UserResponseDto(userNew.Id, userNew.Name, userNew.Email, roleName);
        }

        public async Task<UserResponseDto?> GetAsync(Expression<Func<User, bool>> predicate)
        {
            var user = await _userRepository.GetAsync(predicate);

            if (user == null) 
            {
                throw new KeyNotFoundException("Usuário não encontrado");
            }

            var roleName = ((ERole)user.RoleId).ToString();

            return new UserResponseDto(user.Id, user.Name, user.Email, user.Role.Nome);
        }

        public async Task<bool> UpdateRoleAsync(int userId, int newRoleId)
        {
            var user = await _userRepository.GetAsync(u => u.Id == userId);

            if(user == null)
                throw new KeyNotFoundException("Usuário não encontrado");

            return await _userRepository.UpdateRoleAsync(userId, newRoleId);
        }
    }
}
