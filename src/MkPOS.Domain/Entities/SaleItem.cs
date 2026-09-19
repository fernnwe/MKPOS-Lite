namespace MKPOS.Domain.Entities;

/// <summary>
/// Línea de una venta. Guarda el nombre, precio e impuesto del producto como
/// instantánea para conservar el histórico aunque el producto cambie después.
/// </summary>
public sealed class SaleItem
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    /// <summary>Impuesto porcentual aplicado a esta línea.</summary>
    public decimal TaxRate { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Importe de la línea sin impuesto (precio x cantidad).</summary>
    public decimal LineTotal { get; set; }

    public Sale? Sale { get; set; }
}