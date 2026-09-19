namespace MKPOS.Domain.Entities;

/// <summary>
/// Cliente del negocio. El <see cref="Balance"/> representa la deuda acumulada
/// por ventas a crédito (fiado) pendiente de abonar.
/// </summary>
public sealed class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    /// <summary>Deuda pendiente acumulada por ventas a crédito.</summary>
    public decimal Balance { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}