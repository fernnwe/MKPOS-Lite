using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.Total)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.PaymentAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.ChangeAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.PaymentMethod)
            .HasConversion<int>();

        builder.HasIndex(s => s.TicketNumber)
            .IsUnique();

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}