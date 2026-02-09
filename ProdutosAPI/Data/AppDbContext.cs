using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Models;
using System.Reflection;

namespace ProdutosAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Define a propriedade DbSet para entidades
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplica todas as configurações de entidade do assembly atual
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
                // Conversão de Datetime para trabalhar com UTC
                configurationBuilder.Properties<DateTime>()
                    .HaveConversion(typeof(UtcDateTimeConverter));
        }

        // Classe auxiliar para fazer a conversão
        private class UtcDateTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>
        {
            public UtcDateTimeConverter(): base(
                    // Garante que é UTC
                    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                    // Garante que o C# entenda como UTC
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }
    }
}
