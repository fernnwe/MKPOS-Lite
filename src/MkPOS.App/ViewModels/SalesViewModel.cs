using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using System.Collections.ObjectModel;

namespace MKPOS.App.ViewModels;

public partial class SalesViewModel : ObservableObject, IModuleViewModel
{
    private const int RecentCount = 100;

    private readonly ISaleService _sales;

    public ObservableCollection<SaleDto> Sales { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public SalesViewModel(ISaleService sales)
    {
        _sales = sales;
    }

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _sales.GetRecentAsync(RecentCount);
            Sales.Clear();
            foreach (var sale in list)
            {
                Sales.Add(sale);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}