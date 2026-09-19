using Microsoft.EntityFrameworkCore;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly MKPOSDbContext _db;

    public ReportRepository(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task<PeriodSummaryDto> GetPeriodSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var query = _db.Sales.AsNoTracking().Where(s => s.SaleDate >= from && s.SaleDate < to);

        var totalTickets = await query.CountAsync(ct);
        var cancelledTickets = await query.CountAsync(s => s.IsCancelled, ct);

        var valid = query.Where(s => !s.IsCancelled);
        var subtotal = await valid.SumAsync(s => (decimal?)s.Subtotal, ct) ?? 0m;
        var taxAmount = await valid.SumAsync(s => (decimal?)s.TaxAmount, ct) ?? 0m;
        var total = await valid.SumAsync(s => (decimal?)s.Total, ct) ?? 0m;

        return new PeriodSummaryDto(totalTickets, cancelledTickets, subtotal, taxAmount, total);
    }

    public async Task<IReadOnlyList<DailySalesDto>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var rows = await _db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= from && s.SaleDate < to && !s.IsCancelled)
            .Select(s => new { s.SaleDate, s.Total })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.SaleDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailySalesDto(g.Key, g.Count(), g.Sum(r => r.Total)))
            .ToList();
    }

    public async Task<IReadOnlyList<PaymentMethodSummaryDto>> GetSalesByPaymentMethodAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var rows = await _db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= from && s.SaleDate < to && !s.IsCancelled)
            .Select(s => new { s.PaymentMethod, s.Total })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.PaymentMethod)
            .OrderBy(g => g.Key)
            .Select(g => new PaymentMethodSummaryDto(PaymentMethodName(g.Key), g.Count(), g.Sum(r => r.Total)))
            .ToList();
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken ct = default)
    {
        var rows = await _db.SaleItems.AsNoTracking()
            .Join(
                _db.Sales.AsNoTracking(),
                item => item.SaleId,
                sale => sale.Id,
                (item, sale) => new { item.ProductName, item.Quantity, item.LineTotal, sale.SaleDate, sale.IsCancelled })
            .Where(r => r.SaleDate >= from && r.SaleDate < to && !r.IsCancelled)
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.ProductName)
            .Select(g => new TopProductDto(g.Key, g.Sum(r => r.Quantity), g.Sum(r => r.LineTotal)))
            .OrderByDescending(p => p.Quantity)
            .ThenByDescending(p => p.Revenue)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<CustomerBalanceDto>> GetCustomerBalancesAsync(CancellationToken ct = default)
    {
        return await _db.Customers.AsNoTracking()
            .Where(c => c.IsActive && c.Balance > 0)
            .OrderByDescending(c => c.Balance)
            .Select(c => new CustomerBalanceDto(c.Name, c.Phone, c.Balance))
            .ToListAsync(ct);
    }

    private static string PaymentMethodName(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => "Efectivo",
            PaymentMethod.Card => "Tarjeta",
            PaymentMethod.Transfer => "Transferencia",
            PaymentMethod.Credit => "Crédito",
            _ => method.ToString()
        };
    }
}