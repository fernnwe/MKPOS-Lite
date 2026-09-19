namespace MKPOS.Infrastructure;

/// <summary>
/// Ubicaciones de archivos en disco usadas por la aplicación.
/// En configuración, no es necesario elevación de privilegios.
/// </summary>
public static class AppPaths
{
    private static readonly string BaseDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MKPOS");

    public static string DataDirectory => BaseDirectory;

    public static string DatabasePath => Path.Combine(BaseDirectory, "mkpos.db");

    public static string LogsDirectory => Path.Combine(BaseDirectory, "logs");

    public static string BackupsDirectory => Path.Combine(BaseDirectory, "backups");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BaseDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
    }
}