using MKPOS.Application.DTOs;

namespace MKPOS.Application.Services;

public interface ISaleService
{
    Task<CreateSaleResult> CompleteAsync(CreateSaleRequest request, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<SaleDto>> GetRecentAsync(int take, CancellationToken ct = default);
}