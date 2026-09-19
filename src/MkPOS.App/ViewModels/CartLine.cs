using CommunityToolkit.Mvvm.ComponentModel;

namespace MKPOS.App.ViewModels;

/// <summary>
/// Línea del carrito del punto de venta. Recalcula sus importes al cambiar
/// la cantidad.
/// </summary>
public sealed partial class CartLine : ObservableObject
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }

    [ObservableProperty]
    private decimal _quantity;

    public decimal LineSubtotal => Quantity * UnitPrice;
    public decimal LineTax => LineSubtotal * TaxRate / 100m;
    public decimal LineTotal => LineSubtotal + LineTax;

    public CartLine(Guid productId, string name, decimal unitPrice, decimal taxRate, decimal quantity)
    {
        ProductId = productId;
        Name = name;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        Quantity = quantity;
    }

    partial void OnQuantityChanged(decimal value)
    {
        OnPropertyChanged(nameof(LineSubtotal));
        OnPropertyChanged(nameof(LineTax));
        OnPropertyChanged(nameof(LineTotal));
    }
}