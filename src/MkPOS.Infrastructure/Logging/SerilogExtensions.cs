using Serilog;

namespace MKPOS.Infrastructure.Logging;

public static class SerilogExtensions
{
    /// <summary>
    /// Configura el logger estático de Serilog con salida a archivos rotativos.
    /// </summary>
    public static void ConfigureSerilog()
    {
        AppPaths.EnsureDirectoriesExist();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(AppPaths.LogsDirectory, "mkpos-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                formatProvider: System.Globalization.CultureInfo.InvariantCulture)
            .CreateLogger();
    }
}