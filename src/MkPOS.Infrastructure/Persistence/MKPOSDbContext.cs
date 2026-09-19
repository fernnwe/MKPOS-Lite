using Microsoft.EntityFrameworkCore;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence;

public sealed class MKPOSDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    public MKPOSDbContext(DbContextOptions<MKPOSDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MKPOSDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}