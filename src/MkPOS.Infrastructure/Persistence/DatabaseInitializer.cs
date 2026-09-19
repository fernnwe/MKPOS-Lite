using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MKPOS.Infrastructure.Persistence;

/// <summary>
/// Aplica migraciones y crea la estructura de la base de datos al arrancar.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly MKPOSDbContext _db;

    public DatabaseInitializer(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();

        Log.Information("Aplicando migraciones sobre {DatabasePath}", AppPaths.DatabasePath);
        await _db.Database.MigrateAsync(ct);
    }
}