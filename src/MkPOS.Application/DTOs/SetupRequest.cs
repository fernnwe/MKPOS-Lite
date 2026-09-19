namespace MKPOS.Application.DTOs;

/// <summary>
/// Datos capturados durante la configuración inicial (primera ejecución).
/// </summary>
public sealed record SetupRequest(
    string CompanyName,
    string? TaxId,
    string? Address,
    string? Phone,
    string? Email,
    string CurrencySymbol,
    decimal TaxRate,
    string? ReceiptFooter,
    string AdminUsername,
    string AdminDisplayName,
    string AdminPassword);