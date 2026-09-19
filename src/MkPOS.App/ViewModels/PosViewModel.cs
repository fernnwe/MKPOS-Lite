using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.App.Services;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using MKPOS.Domain.Entities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;

namespace MKPOS.App.ViewModels;

public partial class PosViewModel : ObservableObject, IModuleViewModel
{
    private readonly IProductService _products;
    private readonly ISaleService _sales;
    private readonly ICompanyRepository _companies;
    private readonly SessionService _session;
    private readonly HashSet<CartLine> _attachedLines = new();

    private decimal _defaultTaxRate;

    public ObservableCollection<ProductDto> Products { get; } = new();
    public ObservableCollection<CartLine> Cart { get; } = new();

    [ObservableProperty]
    private string _search = string.Empty;

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedCommand))]
    private ProductDto? _selectedRow;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isPaying;

    [ObservableProperty]
    private string _paymentAmountText = string.Empty;

    [ObservableProperty]
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public decimal Subtotal => Cart.Sum(l => l.LineSubtotal);
    public decimal TaxAmount => Cart.Sum(l => l.LineTax);
    public decimal Total => Cart.Sum(l => l.LineTotal);

    public decimal Change => SelectedPaymentMethod == PaymentMethod.Cash
        && TryParseDecimal(PaymentAmountText, out var received)
        ? Math.Max(0, received - Total)
        : 0;

    public bool IsCash => SelectedPaymentMethod == PaymentMethod.Cash;

    public PosViewModel(
        IProductService products,
        ISaleService sales,
        ICompanyRepository companies,
        SessionService session)
    {
        _products = products;
        _sales = sales;
        _companies = companies;
        _session = session;

        Cart.CollectionChanged += OnCartCollectionChanged;
    }

    public async Task LoadAsync()
    {
        var company = await _companies.GetCurrentAsync();
        _defaultTaxRate = company?.TaxRate ?? 0m;
        await SearchAsync();
    }

    partial void OnSelectedPaymentMethodChanged(PaymentMethod value)
    {
        OnPropertyChanged(nameof(IsCash));
        OnPropertyChanged(nameof(Change));
    }

    partial void OnPaymentAmountTextChanged(string value)
    {
        OnPropertyChanged(nameof(Change));
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _products.GetProductsAsync(Search, null, includeInactive: false);
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

    [RelayCommand(CanExecute = nameof(CanAddSelected))]
    private void AddSelected()
    {
        if (SelectedRow is not null)
        {
            AddToCart(SelectedRow);
        }
    }

    private bool CanAddSelected() => SelectedRow is not null;

    [RelayCommand]
    private void AddProduct(ProductDto? product)
    {
        if (product is not null)
        {
            AddToCart(product);
        }
    }

    [RelayCommand]
    private void RemoveLine(CartLine? line)
    {
        if (line is null)
        {
            return;
        }

        Cart.Remove(line);
        NotifyCartTotals();
    }

    [RelayCommand]
    private void Increase(CartLine? line)
    {
        if (line is null)
        {
            return;
        }

        line.Quantity += 1;
        NotifyCartTotals();
    }

    [RelayCommand]
    private void Decrease(CartLine? line)
    {
        if (line is null)
        {
            return;
        }

        if (line.Quantity <= 1)
        {
            Cart.Remove(line);
        }
        else
        {
            line.Quantity -= 1;
        }

        NotifyCartTotals();
    }

    private void AddToCart(ProductDto product)
    {
        if (product.Stock <= 0)
        {
            ErrorMessage = $"Sin existencias de \"{product.Name}\".";
            return;
        }

        var existing = Cart.FirstOrDefault(l => l.ProductId == product.Id);
        if (existing is not null)
        {
            if (existing.Quantity >= product.Stock)
            {
                ErrorMessage = $"Solo hay {product.Stock:N2} disponibles de \"{product.Name}\".";
                return;
            }

            existing.Quantity += 1;
        }
        else
        {
            var taxRate = product.TaxRate ?? _defaultTaxRate;
            Cart.Add(new CartLine(product.Id, product.Name, product.SalePrice, taxRate, 1));
        }

        ErrorMessage = string.Empty;
        NotifyCartTotals();
    }

    [RelayCommand]
    private void Pay()
    {
        if (Cart.Count == 0)
        {
            ErrorMessage = "El carrito está vacío.";
            return;
        }

        ErrorMessage = string.Empty;
        Message = string.Empty;
        PaymentAmountText = string.Empty;
        IsPaying = true;
    }

    [RelayCommand]
    private void CancelPayment()
    {
        IsPaying = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ConfirmPaymentAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (Cart.Count == 0)
        {
            ErrorMessage = "El carrito está vacío.";
            return;
        }

        ErrorMessage = string.Empty;

        decimal paymentAmount = 0;
        if (SelectedPaymentMethod == PaymentMethod.Cash)
        {
            if (!TryParseDecimal(PaymentAmountText, out paymentAmount))
            {
                ErrorMessage = "Escriba la cantidad recibida en efectivo (ej.: 100).";
                return;
            }
        }

        var request = new CreateSaleRequest
        {
            Lines = Cart.Select(l => new SaleLineInput(l.ProductId, l.Quantity)).ToList(),
            PaymentMethod = SelectedPaymentMethod,
            PaymentAmount = paymentAmount
        };

        IsBusy = true;
        try
        {
            var result = await _sales.CompleteAsync(request, _session.CurrentUser?.Id ?? Guid.Empty);

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "No se pudo completar la venta.";
                return;
            }

            IsPaying = false;
            Cart.Clear();
            PaymentAmountText = string.Empty;
            Message = result.ChangeAmount > 0
                ? $"Venta completada. Folio {result.TicketNumber} - Total {result.Total:N2}. Cambio a devolver: {result.ChangeAmount:N2}."
                : $"Venta completada. Folio {result.TicketNumber} - Total {result.Total:N2}.";

            await SearchAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnCartCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (CartLine line in e.OldItems)
            {
                line.PropertyChanged -= OnCartLinePropertyChanged;
                _attachedLines.Remove(line);
            }
        }

        if (e.NewItems is not null)
        {
            foreach (CartLine line in e.NewItems)
            {
                line.PropertyChanged += OnCartLinePropertyChanged;
                _attachedLines.Add(line);
            }
        }

        NotifyCartTotals();
    }

    private void OnCartLinePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        NotifyCartTotals();
    }

    private void NotifyCartTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(Change));
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
            || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = 0;
        return false;
    }
}