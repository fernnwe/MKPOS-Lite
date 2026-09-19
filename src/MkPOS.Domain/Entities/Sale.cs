namespace MKPOS.Domain.Entities;

/// <summary>
/// Venta cerrada en el punto de venta. Guarda un ticket con folio correlativo
/// y las líneas vendidas (inmutables como instantánea del precio e impuesto).
/// </summary>
public sealed class Sale
{
    public Guid Id { get; set; }

    /// <summary>Folio correlativo de venta del negocio.</summary>
    public int TicketNumber { get; set; }

    public Guid UserId { get; set; }
    public DateTime SaleDate { get; set; }

    /// <summary>Suma de las líneas sin impuesto.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Suma del impuesto de todas las líneas.</summary>
    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }

    /// <summary>Cliente asociado cuando la venta es a crédito; null en ventas de mostrador.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Instantánea del nombre del cliente (null en ventas de mostrador).</summary>
    public string? CustomerName { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}