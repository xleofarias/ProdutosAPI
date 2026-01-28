using Moq;
using System.Linq.Expressions;
using ProdutosAPI.Models;
using ProdutosAPI.DTOs;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services;

namespace ProdutosAPITests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task GetById_DeveRetornaUsuario_QuandoExiste()
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUserRepository>();

            var userEntity = new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                RoleId = 1,
                Role = new Role { Id = 1, Nome = "Admin" }
            };

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(userEntity);

            // Instanciamos o SERVICE REAL injetando o mock do repositório
            var service = new UserService(mockRepo.Object);

            // ACT - Ação
            var result = await service.GetAsync(p => p.Id == 1);

            //Assert - Verificação
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);

            Assert.Equal("Teste", result.Name);

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once());
        }

        [Fact]
        public async Task GetUsuariosById_DeveLancarExcecao_QuandoNaoExiste() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUserRepository>();

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            // Obtém o objeto simulado
            var service = new UserService(mockRepo.Object);

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAsync(p => p.Id == 99));

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once());
        }

        [Fact]
        public async Task PatchChangeUserRole_DeveRetornarTrue_QuandoSucesso() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUserRepository>();
            var userEntity = new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User"
            };

            mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(userEntity);

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            // Obtém o objeto simulado
            var service = new UserService(mockRepo.Object);

            //Act - Ação
            var resultado = await service.UpdateRoleAsync(1, 2);

            //Assert - Verificação
            Assert.True(resultado);

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task PatchChangeUserRole_DeveLancarExcecao_QuandoUsuarioNaoExiste() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUserRepository>();

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);
            
            // Obtém o objeto simulado
            var service = new UserService(mockRepo.Object);

            ///Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateRoleAsync(99, 2));

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task PostRegistrar_DeveRetornaUsuario_QuandoSucesso() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUserRepository>();
            var requestDto = new UserRequestDto
            {
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User",
                Password = "Senha@123",
                RoleId = 1
            };

            // Simula que o usuário não existe
            mockRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(new User
            {
                Id = 1,
                Name = "Teste",
                Email = "email@gmail.com",
                Login = "User"
            });

            // Obtém o objeto simulado
            var service = new UserService(mockRepo.Object);

            //Act - Ação
            var usuarioNovo = await service.CreateAsync(requestDto);

            //Assert - Verificação
            Assert.Equal("Teste", usuarioNovo.Name);

            //  Confirmar a chamada do método
            mockRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once());

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once());
        }
    }
}