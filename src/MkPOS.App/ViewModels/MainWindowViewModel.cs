using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.App.Services;
using MKPOS.Application.Abstractions.Repositories;

namespace MKPOS.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly ICompanyRepository _companies;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _userDisplay = string.Empty;

    public event EventHandler? LogoutRequested;

    public MainWindowViewModel(SessionService session, ICompanyRepository companies)
    {
        _session = session;
        _companies = companies;
    }

    public async Task InitializeAsync()
    {
        var company = await _companies.GetCurrentAsync();
        CompanyName = company?.Name ?? "MKPOS Lite";

        UserDisplay = _session.CurrentUser is { } user
            ? user.DisplayName
            : string.Empty;
    }

    [RelayCommand]
    private void Logout()
    {
        _session.EndSession();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}