using ProdutosAPI.Services.Interfaces;
using Moq;
using System.Linq.Expressions;
using ProdutosAPI.Models;
using ProdutosAPI.DTOs;

namespace ProdutosAPITests.Services
{
    public class UsuarioServiceTests
    {
        [Fact]
        public async Task GetUsuariosById_DeveRetornaUsuario_QuandoExiste()
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUsuarioService>();

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.GetUsuariosById(It.IsAny<Expression<Func<Usuarios, bool>>>()))
                .ReturnsAsync(new Usuarios { Id = 1, Nome = "Teste" });

            // Obtém o objeto simulado
            var service = mockRepo.Object;

            //Act - Ação
            var usuario = await service.GetUsuariosById(p => p.Id == 1);

            //Assert - Verificação
            Assert.Equal("Teste", usuario.Nome);

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.GetUsuariosById(It.IsAny<Expression<Func<Usuarios, bool>>>()), Times.Once());
        }

        [Fact]
        public async Task GetUsuariosById_DeveLancarExcecao_QuandoNaoExiste() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUsuarioService>();

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.GetUsuariosById(It.IsAny<Expression<Func<Usuarios, bool >>>())).ThrowsAsync(new KeyNotFoundException("Usuário não encontrado"));

            // Obtém o objeto simulado
            var service = mockRepo.Object;

            //Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetUsuariosById(p => p.Id == 99));

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.GetUsuariosById(It.IsAny<Expression<Func<Usuarios, bool>>>()), Times.Once());
        }

        [Fact]
        public async Task PatchChangeUserRole_DeveRetornarTrue_QuandoSucesso() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUsuarioService>();

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.PatchChangeUserRole(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            // Obtém o objeto simulado
            var service = mockRepo.Object;

            //Act - Ação
            var resultado = await service.PatchChangeUserRole(1, 2);

            //Assert - Verificação
            Assert.True(resultado);

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.PatchChangeUserRole(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task PatchChangeUserRole_DeveLancarExcecao_QuandoUsuarioNaoExiste() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUsuarioService>();

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.PatchChangeUserRole(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Usuário não encontrado"));

            // Obtém o objeto simulado
            var service = mockRepo.Object;

            ///Act - Ação & Assert - Verificação
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.PatchChangeUserRole(99, 2));

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.PatchChangeUserRole(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task PostRegistrar_DeveRetornaUsuario_QuandoSucesso() 
        {
            //Arrange - Preparação
            var mockRepo = new Mock<IUsuarioService>();

            // Configura o comportamento simulado
            mockRepo.Setup(r => r.PostRegistrar(It.IsAny<RegisterUserDTO>())).ReturnsAsync(new Usuarios { Id = 1, Nome = "Teste" });

            // Obtém o objeto simulado
            var service = mockRepo.Object;


            //Act - Ação
            var usuarioNovo = await service.PostRegistrar(new RegisterUserDTO { Email = "teste@gmail.com", Login = "teste", Nome = "teste", Senha = "123"});

            //Assert - Verificação
            Assert.Equal("Teste", usuarioNovo.Nome);

            // Confirmar a chamada do método
            mockRepo.Verify(r => r.PostRegistrar(It.IsAny<RegisterUserDTO>()), Times.Once());
        }
    }
}