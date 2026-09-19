using System.Windows;
using MKPOS.App.Events;
using MKPOS.App.ViewModels;

namespace MKPOS.App.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public event EventHandler<LoginSucceededEventArgs>? LoginSucceeded;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.LoginCompleted += OnLoginCompleted;

        Loaded += (_, _) =>
        {
            UsernameBox.Focus();
        };
    }

    private void OnLoginCompleted(object? sender, LoginSucceededEventArgs e)
    {
        LoginSucceeded?.Invoke(this, e);
    }
}