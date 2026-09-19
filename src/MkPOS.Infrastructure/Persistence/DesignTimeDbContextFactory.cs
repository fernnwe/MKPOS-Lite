using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MKPOS.Infrastructure.Persistence;

/// <summary>
/// Factoria de diseño usada por "dotnet ef" para generar/ejecutar
/// migraciones sin depender del contenedor de DI de la aplicación.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MKPOSDbContext>
{
    public MKPOSDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MKPOSDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options;

        return new MKPOSDbContext(options);
    }
}