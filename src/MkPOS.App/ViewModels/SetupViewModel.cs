using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;

namespace MKPOS.App.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string? _taxId;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private string? _phone;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string _currencySymbol = "$";

    [ObservableProperty]
    private decimal _taxRate = 16m;

    [ObservableProperty]
    private string? _receiptFooter;

    [ObservableProperty]
    private string _adminUsername = string.Empty;

    [ObservableProperty]
    private string _adminDisplayName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public event EventHandler? SetupCompleted;

    public SetupViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task CompleteSetupAsync(string adminPassword, string confirmPassword)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            ErrorMessage = "La contraseña del administrador es obligatoria.";
            return;
        }

        if (adminPassword != confirmPassword)
        {
            ErrorMessage = "Las contraseñas no coinciden.";
            return;
        }

        if (adminPassword.Length < 6)
        {
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres.";
            return;
        }

        IsBusy = true;

        try
        {
            var request = new SetupRequest(
                CompanyName,
                TaxId,
                Address,
                Phone,
                Email,
                CurrencySymbol,
                TaxRate,
                ReceiptFooter,
                AdminUsername,
                AdminDisplayName,
                adminPassword);

            var result = await _authService.CreateFirstAdminAsync(request);

            if (result.Success)
            {
                SetupCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.Error ?? "No se pudo completar la configuración.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}