using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.TaxId).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(150);
        builder.Property(c => c.CurrencySymbol).HasMaxLength(8).IsRequired();
        builder.Property(c => c.TaxRate).HasColumnType("REAL");
        builder.Property(c => c.ReceiptFooter).HasMaxLength(500);

        builder.Property(c => c.CreatedAt);
        builder.Property(c => c.UpdatedAt);
    }
}