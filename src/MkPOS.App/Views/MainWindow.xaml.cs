using System.Windows;
using MKPOS.App.ViewModels;

namespace MKPOS.App.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public event EventHandler? LogoutRequested;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.LogoutRequested += OnLogoutRequested;

        Loaded += async (_, _) => await _viewModel.InitializeAsync();
    }

    private void OnLogoutRequested(object? sender, EventArgs e)
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}