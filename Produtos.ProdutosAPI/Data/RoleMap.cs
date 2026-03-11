using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdutosAPI.Models;
using ProdutosAPI.Enums;

namespace ProdutosAPI.Datas
{
    public class RoleMap : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // Define a tabela
            builder.ToTable("Roles");

            // Define a chave primária
            builder.HasKey(builder => builder.Id);

            // Define a Identity
            builder.Property(builder => builder.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            // Define as Propriedades
            builder.Property(builder => builder.Nome)
                .IsRequired()
                .HasColumnName("Nome")
                .HasColumnType("VARCHAR")
                .HasMaxLength(128);

            builder.HasData(
                new Role
                {
                    Id = (int)ERole.Admin,
                    Nome = "Admin"
                },
                new Role
                {
                    Id = (int)ERole.User,
                    Nome = "User"
                }
            );
        }
    }
}
