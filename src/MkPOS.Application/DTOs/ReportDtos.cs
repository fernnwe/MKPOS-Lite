namespace MKPOS.Application.DTOs;

/// <summary>Totales de ventas de un período (excluye ventas canceladas).</summary>
public sealed record PeriodSummaryDto(
    int TotalTickets,
    int CancelledTickets,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total);

/// <summary>Ventas del día dentro del período.</summary>
public sealed record DailySalesDto(DateTime Date, int Tickets, decimal Total);

/// <summary>Ventas agrupadas por método de pago.</summary>
public sealed record PaymentMethodSummaryDto(
    string PaymentMethodName,
    int Tickets,
    decimal Total);

/// <summary>Producto más vendido dentro del período.</summary>
public sealed record TopProductDto(string ProductName, decimal Quantity, decimal Revenue);

/// <summary>Saldo por cobrar de un cliente activo.</summary>
public sealed record CustomerBalanceDto(string CustomerName, string? Phone, decimal Balance);