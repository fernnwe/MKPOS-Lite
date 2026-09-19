namespace MKPOS.Domain.Entities;

/// <summary>
/// Forma de pago aceptada al momento de cerrar una venta.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Pago en efectivo; puede generar cambio.</summary>
    Cash = 0,

    /// <summary>Tarjeta de débito o crédito.</summary>
    Card = 1,

    /// <summary>Transferencia bancaria o SPEI.</summary>
    Transfer = 2
}