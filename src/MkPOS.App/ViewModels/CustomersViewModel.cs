using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using System.Collections.ObjectModel;

namespace MKPOS.App.ViewModels;

public partial class CustomersViewModel : ObservableObject, IModuleViewModel
{
    private readonly ICustomerService _customers;

    private Guid? _editingId;

    public ObservableCollection<CustomerDto> Customers { get; } = new();

    [ObservableProperty]
    private string _search = string.Empty;

    [ObservableProperty]
    private bool _includeInactive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeactivateCommand))]
    [NotifyCanExecuteChangedFor(nameof(AbonarCommand))]
    private CustomerDto? _selectedRow;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editorTitle = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private bool _isAbonando;

    [ObservableProperty]
    private string _abonoText = string.Empty;

    public CustomersViewModel(ICustomerService customers)
    {
        _customers = customers;
    }

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    partial void OnSearchChanged(string value)
    {
        _ = RefreshAsync();
    }

    partial void OnIncludeInactiveChanged(bool value)
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _customers.GetCustomersAsync(Search, IncludeInactive);
            Customers.Clear();
            foreach (var customer in list)
            {
                Customers.Add(customer);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RunSearch()
    {
        _ = RefreshAsync();
    }

    [RelayCommand]
    private void New()
    {
        _editingId = null;
        EditorTitle = "Nuevo cliente";
        Name = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        ErrorMessage = string.Empty;
        IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit()
    {
        if (SelectedRow is null)
        {
            return;
        }

        var customer = SelectedRow;
        _editingId = customer.Id;
        EditorTitle = $"Editar: {customer.Name}";
        Name = customer.Name;
        Phone = customer.Phone ?? string.Empty;
        Email = customer.Email ?? string.Empty;
        Address = customer.Address ?? string.Empty;
        ErrorMessage = string.Empty;
        IsEditing = true;
    }

    private bool CanEdit() => SelectedRow is not null;

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var input = new CustomerInput(Name, Phone, Email, Address);

        IsBusy = true;
        try
        {
            OperationResult result = _editingId is { } id
                ? await _customers.UpdateAsync(id, input)
                : await _customers.CreateAsync(input);

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "No se pudo guardar el cliente.";
                return;
            }

            IsEditing = false;
            ErrorMessage = string.Empty;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task DeactivateAsync()
    {
        if (SelectedRow is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _customers.DeactivateAsync(SelectedRow.Id);
            SelectedRow = null;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Abonar()
    {
        if (SelectedRow is null)
        {
            return;
        }

        AbonoText = string.Empty;
        IsAbonando = true;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void CancelAbono()
    {
        IsAbonando = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ConfirmAbonoAsync()
    {
        if (SelectedRow is null)
        {
            return;
        }

        if (!decimal.TryParse(AbonoText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out var amount))
        {
            ErrorMessage = "Escriba un monto válido (ej.: 100).";
            return;
        }

        if (amount <= 0)
        {
            ErrorMessage = "El abono debe ser mayor a cero.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _customers.ApplyPaymentAsync(SelectedRow.Id, amount);
            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "No se pudo registrar el abono.";
                return;
            }

            IsAbonando = false;
            ErrorMessage = string.Empty;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}