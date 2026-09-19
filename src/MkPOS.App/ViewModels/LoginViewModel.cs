using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.App.Events;
using MKPOS.Application.Services;
using System.Windows.Controls;

namespace MKPOS.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public event EventHandler<LoginSucceededEventArgs>? LoginCompleted;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync(PasswordBox passwordBox)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var result = await _authService.LoginAsync(Username, passwordBox.Password);

            if (result.Success)
            {
                LoginCompleted?.Invoke(this, new LoginSucceededEventArgs(result));
            }
            else
            {
                ErrorMessage = result.Error ?? "No se pudo iniciar sesión.";
                passwordBox.Clear();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}