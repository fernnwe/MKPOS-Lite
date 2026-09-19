using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using MKPOS.App.Events;
using MKPOS.App.Services;
using MKPOS.App.ViewModels;
using MKPOS.App.Views;
using MKPOS.Application;
using MKPOS.Application.Services;
using MKPOS.Infrastructure;
using MKPOS.Infrastructure.Logging;
using MKPOS.Infrastructure.Persistence;
using Serilog;

namespace MKPOS.App;

/// <summary>
/// Punto de entrada de la aplicación: configura el contenedor de DI,
/// inicializa la base de datos y decide si mostrar la configuración
/// inicial o la pantalla de inicio de sesión.
/// </summary>
public partial class App : System.Windows.Application
{
    private IServiceProvider? _services;
    private MainWindow? _mainWindow;

    public static IServiceProvider Services =>
        ((App)System.Windows.Application.Current)._services!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SerilogExtensions.ConfigureSerilog();
        Log.Information("Iniciando MKPOS Lite...");

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            var initializer = Services.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync();

            var authService = Services.GetRequiredService<IAuthService>();

            if (await authService.NeedsInitialSetupAsync())
            {
                ShowSetupWindow();
            }
            else
            {
                ShowLoginWindow();
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error crítico al iniciar la aplicación.");
            MessageBox.Show(
                "No se pudo iniciar la aplicación. Revise el registro de eventos.",
                "MKPOS Lite",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication();
        services.AddInfrastructure();

        services.AddSingleton<SessionService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginWindow>();

        services.AddTransient<SetupViewModel>();
        services.AddTransient<SetupWindow>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        services.AddTransient<WelcomeViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<CategoriesViewModel>();
        services.AddTransient<InventoryViewModel>();
    }

    public void ShowLoginWindow()
    {
        var login = Services.GetRequiredService<LoginWindow>();
        login.LoginSucceeded += OnLoginSucceeded;
        ConfigureWindowLifecycle(login);
        login.Show();
    }

    public void ShowSetupWindow()
    {
        var setup = Services.GetRequiredService<SetupWindow>();
        setup.SetupCompleted += (_, _) =>
        {
            Log.Information("Configuración inicial completada.");
            MessageBox.Show(
                "Configuración completada. Inicie sesión con el administrador.",
                "MKPOS Lite",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            setup.Close();
            ShowLoginWindow();
        };
        ConfigureWindowLifecycle(setup);
        setup.Show();
    }

    /// <summary>
    /// Cierra la aplicación cuando no queda ninguna ventana visible.
    /// La comprobación se difiere al dispatcher para no apagar la app durante
    /// las transiciones entre ventanas (ej.: cerrar login y abrir principal).
    /// </summary>
    private void ConfigureWindowLifecycle(Window window)
    {
        window.Closed += (_, _) =>
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                if (!Windows.Cast<Window>().Any(w => w.IsVisible))
                {
                    Shutdown();
                }
            });
        };
    }

    private void OnLoginSucceeded(object? sender, LoginSucceededEventArgs e)
    {
        if (sender is Window login)
        {
            login.Close();
        }

        var session = Services.GetRequiredService<SessionService>();
        session.StartSession(e.Result.User!);

        _mainWindow ??= Services.GetRequiredService<MainWindow>();
        _mainWindow.LogoutRequested += OnMainWindowLogoutRequested;
        _mainWindow.Show();
    }

    private void OnMainWindowLogoutRequested(object? sender, EventArgs e)
    {
        _mainWindow?.Hide();
        ShowLoginWindow();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Excepción no controlada.");
        MessageBox.Show(
            $"Ocurrió un error inesperado.\n{e.Exception.Message}",
            "MKPOS Lite",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("MKPOS Lite finalizado.");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}