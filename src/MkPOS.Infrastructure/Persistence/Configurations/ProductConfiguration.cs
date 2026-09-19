using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1024);

        builder.Property(p => p.Barcode)
            .HasMaxLength(64);

        builder.Property(p => p.PurchasePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.SalePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(p => p.Stock)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.MinStock)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.HasIndex(p => p.Barcode);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}