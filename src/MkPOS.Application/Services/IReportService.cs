using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;

namespace MKPOS.Application.Services;

public interface IReportService
{
    Task<PeriodSummaryDto> GetPeriodSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<DailySalesDto>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentMethodSummaryDto>> GetSalesByPaymentMethodAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerBalanceDto>> GetCustomerBalancesAsync(CancellationToken ct = default);
}

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reports;

    public ReportService(IReportRepository reports)
    {
        _reports = reports;
    }

    public Task<PeriodSummaryDto> GetPeriodSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => _reports.GetPeriodSummaryAsync(from, to, ct);

    public Task<IReadOnlyList<DailySalesDto>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => _reports.GetDailySalesAsync(from, to, ct);

    public Task<IReadOnlyList<PaymentMethodSummaryDto>> GetSalesByPaymentMethodAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => _reports.GetSalesByPaymentMethodAsync(from, to, ct);

    public Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken ct = default)
        => _reports.GetTopProductsAsync(from, to, take, ct);

    public Task<IReadOnlyList<CustomerBalanceDto>> GetCustomerBalancesAsync(CancellationToken ct = default)
        => _reports.GetCustomerBalancesAsync(ct);
}