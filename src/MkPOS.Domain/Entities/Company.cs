namespace MKPOS.Domain.Entities;

/// <summary>
/// Datos del negocio/empresa que se capturan durante la configuración inicial.
/// Se mantiene una única fila activa en la tabla Companies.
/// </summary>
public sealed class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string CurrencySymbol { get; set; } = "$";
    public decimal TaxRate { get; set; }
    public string? ReceiptFooter { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}