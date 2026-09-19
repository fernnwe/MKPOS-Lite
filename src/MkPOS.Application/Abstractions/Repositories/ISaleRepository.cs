using MKPOS.Domain.Entities;

namespace MKPOS.Application.Abstractions.Repositories;

public interface ISaleRepository
{
    Task<int> GetLastTicketNumberAsync(CancellationToken ct = default);

    /// <summary>
    /// Guarda la venta y aplica el ajuste de existencias (y de saldo del cliente
    /// en ventas a crédito) de forma transaccional.
    /// </summary>
    Task CompleteAsync(Sale sale, IReadOnlyList<Product> productsToAdjust, Customer? customer, CancellationToken ct = default);

    Task<IReadOnlyList<Sale>> GetRecentAsync(int take, CancellationToken ct = default);
}