using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdutosAPI.Models;

namespace ProdutosAPI.Datas
{
    public class ProdutoMap : IEntityTypeConfiguration<Produtos>
    {
        public void Configure(EntityTypeBuilder<Produtos> builder)
        {
            // Define a tabela
            builder.ToTable("Produtos");

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

            builder.Property(builder => builder.Preco)
                .IsRequired()
                .HasColumnName("Preco")
                .HasColumnType("DECIMAL");

            builder.Property(builder => builder.Quantidade)
                .IsRequired()
                .HasColumnName("Quantidade")
                .HasColumnType("INT");

            builder.HasIndex(builder => builder.Nome)
                .IsUnique();
        }
    }
}
