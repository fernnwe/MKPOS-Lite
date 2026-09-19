using MKPOS.Domain.Entities;

namespace MKPOS.Application.DTOs;

/// <summary>Línea solicitada desde el carrito.</summary>
public sealed record SaleLineInput(Guid ProductId, decimal Quantity);

public sealed class CreateSaleRequest
{
    public List<SaleLineInput> Lines { get; init; } = new();
    public PaymentMethod PaymentMethod { get; init; }
    public decimal PaymentAmount { get; init; }
}

public sealed class CreateSaleResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public int TicketNumber { get; init; }
    public decimal Total { get; init; }
    public decimal ChangeAmount { get; init; }

    public static CreateSaleResult Ok(int ticketNumber, decimal total, decimal change) => new()
    {
        Success = true,
        TicketNumber = ticketNumber,
        Total = total,
        ChangeAmount = change
    };

    public static CreateSaleResult Fail(string error) => new() { Error = error };
}

public sealed record SaleDto(
    Guid Id,
    int TicketNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    string PaymentMethodName,
    bool IsCancelled,
    int ItemCount);

public sealed record SaleItemDto(
    string ProductName,
    decimal UnitPrice,
    decimal TaxRate,
    decimal Quantity,
    decimal LineTotal);