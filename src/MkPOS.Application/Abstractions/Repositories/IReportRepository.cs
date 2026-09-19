using MKPOS.Application.DTOs;

namespace MKPOS.Application.Abstractions.Repositories;

/// <summary>Lecturas agregadas para los reportes de ventas.</summary>
public interface IReportRepository
{
    Task<PeriodSummaryDto> GetPeriodSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<DailySalesDto>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentMethodSummaryDto>> GetSalesByPaymentMethodAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerBalanceDto>> GetCustomerBalancesAsync(CancellationToken ct = default);
}