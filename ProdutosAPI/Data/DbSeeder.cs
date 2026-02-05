using Microsoft.Extensions.Configuration;
using ProdutosAPI.Models;
using ProdutosAPI.Services;
using BCrypt.Net;

namespace ProdutosAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminUser(AppDbContext dbContext, IConfiguration configuration)
        {
            // Caso o usuário já exista
            if (dbContext.Users.Any())
            {
                return;
            }

            var adminPassword = configuration["AdminPassword"];
            var passwordHash = PasswordHelper.Hash(adminPassword);
            // 👇 O X-9 (DEBUG): O que estamos tentando salvar?
            Console.WriteLine($"[DEBUG SEEDER] Senha usada: {adminPassword}");
            Console.WriteLine($"[DEBUG SEEDER] Hash gerado: {passwordHash}");
            Console.WriteLine($"[DEBUG SEEDER] Tamanho do Hash gerado: {passwordHash.Length}");
            var adminUser = new User
            {
                Name = "Admin",
                Login = "leomw2",
                Email = "leofarias.bliz@gmail.com",
                RoleId = 1,
                Slug = "Administrador",

                //Geração do hash da senha
                PasswordHash = passwordHash
            };

            await dbContext.Users.AddAsync(adminUser);
            await dbContext.SaveChangesAsync();
        }
    }
}
