using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using MKPOS.Domain.Entities;
using MKPOS.Tests.Helpers;

namespace MKPOS.Tests;

public sealed class ReportsServiceTests
{
    private static Sale NewSale(
        int ticket,
        DateTime when,
        PaymentMethod method,
        decimal subtotal = 100m,
        decimal tax = 16m,
        bool cancelled = false,
        params (string Product, decimal Quantity, decimal LineTotal)[] items)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticket,
            SaleDate = when,
            Subtotal = subtotal,
            TaxAmount = tax,
            Total = subtotal + tax,
            PaymentMethod = method,
            IsCancelled = cancelled,
            Items = items
                .Select(i => new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductName = i.Product,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                })
                .ToList()
        };
    }

    private static ReportService CreateSut(IEnumerable<Sale> sales, IEnumerable<Customer>? customers = null)
    {
        return new ReportService(new FakeReportRepository(sales, customers));
    }

    [Fact]
    public async Task GetPeriodSummaryAsync_ExcludesCancelledAndFiltersRange()
    {
        var day = new DateTime(2026, 9, 19, 22, 0, 0);
        var sut = CreateSut(new[]
        {
            NewSale(1, day, PaymentMethod.Cash),
            NewSale(2, day.AddHours(1), PaymentMethod.Credit),
            NewSale(3, day.AddHours(2), PaymentMethod.Cash, cancelled: true),
            NewSale(4, day.AddDays(5), PaymentMethod.Cash)
        });

        var summary = await sut.GetPeriodSummaryAsync(day.Date, day.Date.AddDays(2));

        Assert.Equal(3, summary.TotalTickets);
        Assert.Equal(1, summary.CancelledTickets);
        Assert.Equal(200m, summary.Subtotal);
        Assert.Equal(32m, summary.TaxAmount);
        Assert.Equal(232m, summary.Total);
    }

    [Fact]
    public async Task GetDailySalesAsync_GroupsByDay()
    {
        var day = new DateTime(2026, 9, 19, 10, 0, 0);
        var sut = CreateSut(new[]
        {
            NewSale(1, day, PaymentMethod.Cash),
            NewSale(2, day.AddHours(2), PaymentMethod.Credit),
            NewSale(3, day.AddDays(1), PaymentMethod.Cash)
        });

        var daily = await sut.GetDailySalesAsync(day.Date, day.Date.AddDays(2));

        Assert.Equal(2, daily.Count);
        Assert.Equal(2, daily[0].Tickets);
        Assert.Equal(232m, daily[0].Total);
        Assert.Equal(1, daily[1].Tickets);
    }

    [Fact]
    public async Task GetSalesByPaymentMethodAsync_GroupsByMethod()
    {
        var day = new DateTime(2026, 9, 19);
        var sut = CreateSut(new[]
        {
            NewSale(1, day, PaymentMethod.Cash),
            NewSale(2, day, PaymentMethod.Credit),
            NewSale(3, day, PaymentMethod.Cash)
        });

        var byMethod = await sut.GetSalesByPaymentMethodAsync(day.Date, day.Date.AddDays(1));

        Assert.Equal(2, byMethod.Count);
        Assert.Equal("Cash", byMethod[0].PaymentMethodName);
        Assert.Equal(2, byMethod[0].Tickets);
        Assert.Equal("Credit", byMethod[1].PaymentMethodName);
        Assert.Equal(1, byMethod[1].Tickets);
    }

    [Fact]
    public async Task GetTopProductsAsync_OrdersByQuantityAndTakesLimit()
    {
        var day = new DateTime(2026, 9, 19);
        var sut = CreateSut(new[]
        {
            NewSale(1, day, PaymentMethod.Cash, items: ("Refresco", 2m, 40m)),
            NewSale(2, day, PaymentMethod.Cash, items: ("Botana", 3m, 30m)),
            NewSale(3, day, PaymentMethod.Cash, items: ("Refresco", 1m, 20m)),
            NewSale(4, day.AddDays(3), PaymentMethod.Cash, items: ("Fuera", 99m, 99m))
        });

        var top = await sut.GetTopProductsAsync(day.Date, day.Date.AddDays(1), 2);

        Assert.Equal(2, top.Count);
        Assert.Equal("Refresco", top[0].ProductName);
        Assert.Equal(3m, top[0].Quantity);
        Assert.Equal(60m, top[0].Revenue);
        Assert.Equal("Botana", top[1].ProductName);
    }

    [Fact]
    public async Task GetCustomerBalancesAsync_ReturnsOnlyActiveDebtors()
    {
        var sut = CreateSut(Array.Empty<Sale>(), new[]
        {
            new Customer { Id = Guid.NewGuid(), Name = "Debe", Balance = 150m, IsActive = true },
            new Customer { Id = Guid.NewGuid(), Name = "No Debe", Balance = 0m, IsActive = true },
            new Customer { Id = Guid.NewGuid(), Name = "Inactivo", Balance = 80m, IsActive = false }
        });

        var balances = await sut.GetCustomerBalancesAsync();

        var item = Assert.Single(balances);
        Assert.Equal("Debe", item.CustomerName);
        Assert.Equal(150m, item.Balance);
    }

    [Fact]
    public async Task GetPeriodSummaryAsync_EmptyRange_ReturnsZeros()
    {
        var day = new DateTime(2026, 9, 19);
        var sut = CreateSut(Array.Empty<Sale>());

        var summary = await sut.GetPeriodSummaryAsync(day.Date, day.Date.AddDays(1));

        Assert.Equal(0, summary.TotalTickets);
        Assert.Equal(0m, summary.Subtotal);
        Assert.Equal(0m, summary.Total);
    }
}