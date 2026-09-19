using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MKPOS.App.ViewModels;

/// <summary>
/// Fila de inventario: envuelve un <see cref="ProductDto"/> y expone el
/// estado calculado de existencia para su presentación.
/// </summary>
public sealed class ProductRow
{
    public ProductRow(ProductDto product)
    {
        Product = product;
    }

    public ProductDto Product { get; }

    public Guid Id => Product.Id;
    public string Name => Product.Name;
    public string Sku => Product.Sku;
    public string? CategoryName => Product.CategoryName;
    public decimal Stock => Product.Stock;
    public decimal MinStock => Product.MinStock;
    public decimal CostValue => Product.PurchasePrice;

    public string Status => Product.Stock <= 0
        ? "Sin existencias"
        : Product.Stock < Product.MinStock
            ? "Bajo stock"
            : "Suficiente";

    public string StatusColor => Product.Stock <= 0
        ? "#DC2626"
        : Product.Stock < Product.MinStock
            ? "#D97706"
            : "#16A34A";
}

public partial class InventoryViewModel : ObservableObject, IModuleViewModel
{
    private readonly IProductService _products;

    public ObservableCollection<ProductRow> Rows { get; } = new();

    [ObservableProperty]
    private string _search = string.Empty;

    [ObservableProperty]
    private bool _onlyLowStock;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isAdjusting;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _adjustStockText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdjustCommand))]
    private ProductRow? _selectedRow;

    public InventoryViewModel(IProductService products)
    {
        _products = products;
    }

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    partial void OnOnlyLowStockChanged(bool value)
    {
        _ = RefreshAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _products.GetProductsAsync(Search, null, includeInactive: true);
            var filtered = OnlyLowStock
                ? list.Where(p => p.Stock <= 0 || p.Stock < p.MinStock)
                : list;

            Rows.Clear();
            foreach (var product in filtered)
            {
                Rows.Add(new ProductRow(product));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAdjust))]
    private void Adjust()
    {
        if (SelectedRow is null)
        {
            return;
        }

        AdjustStockText = SelectedRow.Stock.ToString(CultureInfo.CurrentCulture);
        ErrorMessage = string.Empty;
        IsAdjusting = true;
    }

    private bool CanAdjust() => SelectedRow is not null;

    [RelayCommand]
    private void CancelAdjust()
    {
        IsAdjusting = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAdjustAsync()
    {
        if (SelectedRow is null || !decimal.TryParse(AdjustStockText, NumberStyles.Number, CultureInfo.CurrentCulture, out var newStock))
        {
            ErrorMessage = "Escriba una existencia válida (ej.: 25).";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _products.AdjustStockAsync(SelectedRow.Id, newStock);
            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "No se pudo ajustar la existencia.";
                return;
            }

            IsAdjusting = false;
            ErrorMessage = string.Empty;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}