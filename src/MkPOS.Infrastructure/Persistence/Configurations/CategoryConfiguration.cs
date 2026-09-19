using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(512);

        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}