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

            var adminUser = new User
            {
                Name = "Admin",
                Login = "leomw2",
                Email = "leofarias.bliz@gmail.com",
                RoleId = 1,

                //Geração do hash da senha
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(configuration["AdminPassword"])
            };

            await dbContext.Users.AddAsync(adminUser);
            await dbContext.SaveChangesAsync();
        }
    }
}
