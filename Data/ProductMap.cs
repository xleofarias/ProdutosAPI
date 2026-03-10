using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdutosAPI.Models;

namespace ProdutosAPI.Data
{
    public class ProductMap : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Define a tabela
            builder.ToTable("Products");

            // Define a chave primária
            builder.HasKey(x => x.Id);

            // Define a Identity
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            // Define as Propriedades
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Price)
                .IsRequired()
                .HasPrecision(18,2);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}
