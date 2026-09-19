using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.PasswordSalt)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.CreatedAt);
        builder.Property(u => u.UpdatedAt);
    }
}