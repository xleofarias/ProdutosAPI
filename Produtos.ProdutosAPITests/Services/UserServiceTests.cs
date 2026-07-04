using Moq;
using System.Linq.Expressions;
using ProdutosAPI.Models;
using ProdutosAPI.DTOs;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MassTransit;
using Contracts.Events;
using ProdutosAPITests.DTOsTests;

namespace ProdutosAPITests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<ISendEndpointProvider> _mockPublish;
        private readonly Mock<ILogger<User>> _mockLogger;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockCache = new Mock<IDistributedCache>();
            _mockPublish = new Mock<ISendEndpointProvider>();
            _mockLogger = new Mock<ILogger<User>>();

            _service = new UserService(_mockRepo.Object, _mockCache.Object, _mockPublish.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetById_DeveRetornaUsuario_QuandoExiste()
        {
            //Arrange - Prepara��o
            var userEntity = new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User",
                PasswordHash = "hashedpassword",
                Slug = "teste",
                RoleId = 1,
                Role = new Role { Id = 1, Nome = "Admin" }
            };

            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userEntity);

            // ACT - Ação    
            var result = await _service.GetAsync(p => p.Id == 1);

            //Assert - Verifica��o
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);

            Assert.Equal("Teste", result.Name);

            // Confirmar a chamada do m�todo
            _mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task GetUsuariosById_DeveLancarExcecao_QuandoNaoExiste() 
        {
            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>() ))
                .ReturnsAsync((User?)null);

            //Act - A��o & Assert - Verifica��o
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAsync(p => p.Id == 99));

            // Confirmar a chamada do m�todo
            _mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Once());
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
                PasswordHash = "hashedpassword",
                Slug = "teste",
                RoleId = 1,
                Role = new Role { Id = 1, Nome ="Admin"}
            };

            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userEntity);

            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            //Act - Ação
            var resultado = await _service.UpdateRoleAsync(1, 2);

            //Assert - Verificação
            Assert.True(resultado);

            // Confirmar a chamada do método
            _mockRepo.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once());

            _mockPublish.Verify(p => p.Send(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PatchChangeUserRole_DeveLancarExcecao_QuandoUsuarioNaoExiste() 
        {
            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            
            ///Act - A��o & Assert - Verifica��o
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateRoleAsync(99, 2));

            // Confirmar a chamada do m�todo
            _mockRepo.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _mockPublish.Verify(p => p.Send(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PostRegistrar_DeveRetornaUsuario_QuandoSucesso() 
        {
            //Arrange - Prepara��o
            var requestDto = new UserRequestDto
            {
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User",
                Password = "Senha@123",
                RoleId = 1
            };

            // Simula que o usu�rio n�o existe
            _mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>() ))
                .ReturnsAsync((User?)null);

            // Configura o comportamento simulado
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User",
                PasswordHash = "hashedpassword",
                Slug = "teste"
            });

            //Act - A��o
            var usuarioNovo = await _service.CreateAsync(requestDto);

            //Assert - Verificação
            Assert.Equal("Teste", usuarioNovo.Name);

            //  Confirmar a chamada do método
            _mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Once());

            // Confirmar a chamada do método
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once());

            _mockPublish.Verify(p => p.Send(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}