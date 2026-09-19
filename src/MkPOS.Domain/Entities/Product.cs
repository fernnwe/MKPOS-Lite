namespace MKPOS.Domain.Entities;

/// <summary>
/// Producto vendido en el punto de venta. La existencia se mantiene en
/// <see cref="Stock"/> de forma directa para mantener el inventario simple;
/// el historial de movimientos se añadirá en fases posteriores.
/// </summary>
public sealed class Product
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }

    /// <summary>Impuesto porcentual propio; si es null se usa el de la empresa.</summary>
    public decimal? TaxRate { get; set; }

    public decimal Stock { get; set; }
    public decimal MinStock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Category? Category { get; set; }
}