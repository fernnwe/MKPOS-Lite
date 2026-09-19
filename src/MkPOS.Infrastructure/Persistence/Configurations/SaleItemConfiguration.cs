using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(i => i.Quantity)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.LineTotal)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(i => i.SaleId);
    }
}