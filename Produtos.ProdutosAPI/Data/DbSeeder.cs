using ProdutosAPI.Models;
using ProdutosAPI.Services;

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

            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? configuration["AdminPassword"] ?? "admin123";
            var passwordHash = PasswordHelper.Hash(adminPassword);
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
