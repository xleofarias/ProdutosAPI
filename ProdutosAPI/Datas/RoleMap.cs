using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdutosAPI.Models;

namespace ProdutosAPI.Datas
{
    public class RoleMap : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
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
        }
    }
}
