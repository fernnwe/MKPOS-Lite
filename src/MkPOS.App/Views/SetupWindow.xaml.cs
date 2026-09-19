using System.Windows;
using MKPOS.App.ViewModels;

namespace MKPOS.App.Views;

public partial class SetupWindow : Window
{
    private readonly SetupViewModel _viewModel;

    public event EventHandler? SetupCompleted;

    public SetupWindow(SetupViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.SetupCompleted += OnSetupCompleted;

        // PasswordBox no permite binding directo de Password; se pasa por code-behind.
        CompleteButton.Click += HandleCompleteClick;
    }

    private async void HandleCompleteClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.CompleteSetupAsync(AdminPasswordBox.Password, ConfirmPasswordBox.Password);
    }

    private void OnSetupCompleted(object? sender, EventArgs e)
    {
        SetupCompleted?.Invoke(this, EventArgs.Empty);
    }
}