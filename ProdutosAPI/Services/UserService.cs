using ProdutosAPI.DTOs;
using ProdutosAPI.Enums;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services.Interfaces;
using System.Linq.Expressions;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;
using ProdutosAPI.Extensions;

namespace ProdutosAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDistributedCache _cache;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<User> _logger;
        public UserService(IUserRepository userRepository, IDistributedCache cache, IPublishEndpoint publishEndpoint, ILogger<User> logger)
        {
            _userRepository = userRepository;
            _cache = cache;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }
        public async Task<UserResponseDto> CreateAsync(UserRequestDto user)
        {
            var existingUser = await _userRepository.GetAsync(u => u.Email == user.Email || u.Login == user.Login);

            if (existingUser != null)
            {
                // Melhoria: Feedback específico
                if (existingUser.Email == user.Email)
                    throw new ConflictException("Email already registered.");

                if (existingUser.Login == user.Login)
                    throw new ConflictException("Login already registered.");
            }

            var userNew = new User
            {
                Name = user.Name,
                Email = user.Email,
                Login = user.Login,
                PasswordHash = PasswordHelper.Hash(user.Password),
                Slug = user.Name.ToLower().Replace(" ", "-"),
                RoleId = user.RoleId
            };

            var roleName = ((ERole)userNew.RoleId).ToString();

            try
            {
                await _userRepository.CreateAsync(userNew);

                var evento = new UserCreatedEvent(userNew.Id, userNew.Name, DateTime.UtcNow);

                await _publishEndpoint.Publish(evento);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error registering user: {ex.Message}");
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

            var updateUser = await _userRepository.UpdateRoleAsync(userId, newRoleId);

            var evento = new UserCreatedEvent(user.Id, user.Name, DateTime.UtcNow);

            return updateUser;
        }
    }
}
