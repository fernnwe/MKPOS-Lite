using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MKPOS.App.Services;
using MKPOS.Application.Abstractions.Repositories;

namespace MKPOS.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly ICompanyRepository _companies;
    private readonly IServiceProvider _services;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _userDisplay = string.Empty;

    [ObservableProperty]
    private object? _currentModule;

    public event EventHandler? LogoutRequested;

    public MainWindowViewModel(
        SessionService session,
        ICompanyRepository companies,
        IServiceProvider services)
    {
        _session = session;
        _companies = companies;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        var company = await _companies.GetCurrentAsync();
        CompanyName = company?.Name ?? "MKPOS Lite";

        UserDisplay = _session.CurrentUser is { } user
            ? user.DisplayName
            : string.Empty;

        await NavigateToAsync<WelcomeViewModel>();
    }

    [RelayCommand]
    private void Logout()
    {
        _session.EndSession();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private Task GoToProductsAsync() => NavigateToAsync<ProductsViewModel>();

    [RelayCommand]
    private Task GoToCategoriesAsync() => NavigateToAsync<CategoriesViewModel>();

    [RelayCommand]
    private Task GoToInventoryAsync() => NavigateToAsync<InventoryViewModel>();

    [RelayCommand]
    private Task GoToPosAsync() => NavigateToAsync<PosViewModel>();

    [RelayCommand]
    private Task GoToSalesAsync() => NavigateToAsync<SalesViewModel>();

    [RelayCommand]
    private Task GoToCustomersAsync() => NavigateToAsync<CustomersViewModel>();

    private async Task NavigateToAsync<TModule>() where TModule : IModuleViewModel
    {
        var module = _services.GetRequiredService<TModule>();
        await module.LoadAsync();
        CurrentModule = module;
    }
}