using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MKPOS.App.ViewModels;

public partial class ProductsViewModel : ObservableObject, IModuleViewModel
{
    private static readonly CategoryDto NoCategory = new(Guid.Empty, "— Sin categoría —", null, 0, false);

    private readonly IProductService _products;
    private readonly ICategoryService _categories;

    private Guid? _editingId;

    public ObservableCollection<ProductDto> Products { get; } = new();
    public ObservableCollection<CategoryDto> CategoryOptions { get; } = new();

    [ObservableProperty]
    private string _search = string.Empty;

    [ObservableProperty]
    private bool _includeInactive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private ProductDto? _selectedRow;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editorTitle = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _sku = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _barcode = string.Empty;

    [ObservableProperty]
    private string _purchasePrice = string.Empty;

    [ObservableProperty]
    private string _salePrice = string.Empty;

    [ObservableProperty]
    private string _taxRate = string.Empty;

    [ObservableProperty]
    private string _stock = string.Empty;

    [ObservableProperty]
    private string _minStock = string.Empty;

    [ObservableProperty]
    private bool _editorIsActive = true;

    [ObservableProperty]
    private CategoryDto? _selectedCategoryOption;

    public ProductsViewModel(IProductService products, ICategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task LoadAsync()
    {
        await LoadCategoriesAsync();
        await RefreshAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _categories.GetCategoriesAsync();
        CategoryOptions.Clear();
        CategoryOptions.Add(NoCategory);
        foreach (var category in categories)
        {
            CategoryOptions.Add(category);
        }
    }

    partial void OnIncludeInactiveChanged(bool value)
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
            var list = await _products.GetProductsAsync(Search, null, IncludeInactive);
            Products.Clear();
            foreach (var product in list)
            {
                Products.Add(product);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        _editingId = null;
        EditorTitle = "Nuevo producto";
        Sku = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Barcode = string.Empty;
        PurchasePrice = "0";
        SalePrice = "0";
        TaxRate = string.Empty;
        Stock = "0";
        MinStock = "0";
        EditorIsActive = true;
        SelectedCategoryOption = NoCategory;
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

        var p = SelectedRow;
        _editingId = p.Id;
        EditorTitle = $"Editar: {p.Name}";
        Sku = p.Sku;
        Name = p.Name;
        Description = p.Description ?? string.Empty;
        Barcode = p.Barcode ?? string.Empty;
        PurchasePrice = p.PurchasePrice.ToString(CultureInfo.CurrentCulture);
        SalePrice = p.SalePrice.ToString(CultureInfo.CurrentCulture);
        TaxRate = p.TaxRate?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        Stock = p.Stock.ToString(CultureInfo.CurrentCulture);
        MinStock = p.MinStock.ToString(CultureInfo.CurrentCulture);
        EditorIsActive = p.IsActive;
        SelectedCategoryOption = CategoryOptions.FirstOrDefault(c => c.Id == p.CategoryId) ?? NoCategory;
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

        if (!TryParseDecimal(SalePrice, out var salePrice))
        {
            ErrorMessage = "Escriba un precio de venta válido (ej.: 99.50).";
            return;
        }

        if (!TryParseDecimal(PurchasePrice, out var purchasePrice))
        {
            ErrorMessage = "Escriba un precio de compra válido (ej.: 49.25).";
            return;
        }

        decimal? taxRate = null;
        if (!string.IsNullOrWhiteSpace(TaxRate))
        {
            if (!TryParseDecimal(TaxRate, out var tax) || tax < 0)
            {
                ErrorMessage = "Escriba un impuesto válido (ej.: 16).";
                return;
            }

            taxRate = tax;
        }

        if (!TryParseDecimal(Stock, out var stock))
        {
            ErrorMessage = "Escriba una existencia válida (ej.: 10).";
            return;
        }

        if (!TryParseDecimal(MinStock, out var minStock))
        {
            ErrorMessage = "Escriba un nivel mínimo válido.";
            return;
        }

        Guid? categoryId = SelectedCategoryOption is { Id: { } cid } && cid != Guid.Empty ? cid : null;

        var input = new ProductInput(
            Sku,
            Name,
            Description,
            Barcode,
            categoryId,
            purchasePrice,
            salePrice,
            taxRate,
            stock,
            minStock,
            EditorIsActive);

        IsBusy = true;
        try
        {
            OperationResult result = _editingId is { } id
                ? await _products.UpdateAsync(id, input)
                : await _products.CreateAsync(input);

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "No se pudo guardar el producto.";
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
    private async Task DeleteAsync()
    {
        if (SelectedRow is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _products.ToggleActiveAsync(SelectedRow.Id);
            SelectedRow = null;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        var candidates = new[]
        {
            text ?? string.Empty,
            (text ?? string.Empty).Replace(',', '.')
        };

        foreach (var candidate in candidates)
        {
            if (decimal.TryParse(candidate, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
                || decimal.TryParse(candidate, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        value = 0;
        return false;
    }
}