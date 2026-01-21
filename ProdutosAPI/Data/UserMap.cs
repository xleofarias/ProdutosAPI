using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdutosAPI.Models;

namespace ProdutosAPI.Datas
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder) 
        {
            // Define a tabela
            builder.ToTable("Users");

            // Define a chave primária
            builder.HasKey(builder => builder.Id);

            // Define a Identity
            builder.Property(builder => builder.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            // Define as Propriedades
            builder.Property(builder => builder.Name)
                .IsRequired()
                .HasColumnName("Nome")
                .HasColumnType("NVARCHAR")
                .HasMaxLength(80);

            builder.Property(builder => builder.Email)
                .IsRequired()
                .HasColumnName("Email")
                .HasColumnType("VARCHAR")
                .HasMaxLength(160);

            builder.Property(builder => builder.Login)
                .IsRequired()
                .HasColumnName("Login")
                .HasColumnType("VARCHAR")
                .HasMaxLength(20);

            builder.Property(builder => builder.PasswordHash)
                .IsRequired()
                .HasColumnName("PasswordHash")
                .HasColumnType("VARCHAR")
                .HasMaxLength(255);

            builder.Property(builder => builder.Slug)
                .IsRequired()
                .HasColumnName("Slug")
                .HasColumnType("VARCHAR")
                .HasMaxLength(80);

            // Índice único
            builder.HasIndex(builder => builder.Slug, "IX_User_Slug")
                .IsUnique();
        }
    }
}
