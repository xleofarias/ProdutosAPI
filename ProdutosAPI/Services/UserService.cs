using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services.Interfaces;
using SecureIdentity.Password;
using System.Linq.Expressions;

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
                PasswordHash = PasswordHasher.Hash(user.Password),
                Slug = user.Name.ToLower().Replace(" ", "-"),
                RoleId = user.RoleId
            };

            try
            {
                await _userRepository.CreateAsync(userNew);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error registering user: {ex.Message}");
            }

            return new UserResponseDto(userNew.Id, userNew.Name, userNew.Email, userNew.Role.Nome);
        }

        public async Task<UserResponseDto?> GetAsync(Expression<Func<User, bool>> predicate)
        {
            var user = await _userRepository.GetAsync(predicate);



            if (user == null) 
            {
                throw new KeyNotFoundException("Usuário não encontrado");
            }

            return new UserResponseDto(user.Id, user.Name, user.Email, user.Role.Nome);
        }

        public Task<bool> UpdateRoleAsync(int userId, int newRoleId)
        {
            var user = _userRepository.GetAsync(u => u.Id == userId);

            if(user is null)
                throw new KeyNotFoundException("Usuário não encontrado");

            return _userRepository.UpdateRoleAsync(userId, newRoleId);
        }
    }
}