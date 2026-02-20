using Moq;
using System.Linq.Expressions;
using ProdutosAPI.Models;
using ProdutosAPI.DTOs;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace ProdutosAPITests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockCache = new Mock<IDistributedCache>();

            _service = new UserService(_mockRepo.Object, _mockCache.Object);
        }

        [Fact]
        public async Task GetById_DeveRetornaUsuario_QuandoExiste()
        {
            //Arrange - Preparação
            var userEntity = new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                RoleId = 1,
                Role = new Role { Id = 1, Nome = "Admin" }
            };

            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(userEntity);

            // ACT - Ação
            var result = await _service.GetAsync(p => p.Id == 1);

            //Assert - Verificação
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);

            Assert.Equal("Teste", result.Name);

            // Confirmar a chamada do método
            _mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once());
        }

        [Fact]
        public async Task GetUsuariosById_DeveLancarExcecao_QuandoNaoExiste() 
        {
            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAsync(p => p.Id == 99));

            // Confirmar a chamada do método
            _mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once());
        }

        [Fact]
        public async Task PatchChangeUserRole_DeveRetornarTrue_QuandoSucesso() 
        {
            //Arrange - Preparação
            var userEntity = new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User",
                RoleId = 1,
                Role = new Role { Id = 1, Nome ="Admin"}
            };

            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(userEntity);

            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            //Act - Ação
            var resultado = await _service.UpdateRoleAsync(1, 2);

            //Assert - Verificação
            Assert.True(resultado);

            // Confirmar a chamada do método
            _mockRepo.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task PatchChangeUserRole_DeveLancarExcecao_QuandoUsuarioNaoExiste() 
        {
            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);
            
            ///Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateRoleAsync(99, 2));

            // Confirmar a chamada do método
            _mockRepo.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task PostRegistrar_DeveRetornaUsuario_QuandoSucesso() 
        {
            //Arrange - Preparação
            var requestDto = new UserRequestDto
            {
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User",
                Password = "Senha@123",
                RoleId = 1
            };

            // Simula que o usuário não existe
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User"
            });

            //Act - Ação
            var usuarioNovo = await _service.CreateAsync(requestDto);

            //Assert - Verificação
            Assert.Equal("Teste", usuarioNovo.Name);

            //  Confirmar a chamada do método
            _mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once());

            // Confirmar a chamada do método
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once());
        }
    }
}