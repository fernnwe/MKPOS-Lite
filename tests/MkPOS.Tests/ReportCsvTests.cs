using System.Globalization;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;

namespace MKPOS.Tests;

public sealed class ReportCsvTests
{
    [Fact]
    public void BuildTopProducts_ContainsHeaderAndFormattedRows()
    {
        var csv = ReportCsv.BuildTopProducts(new[]
        {
            new TopProductDto("Refresco", 3m, 60m)
        });

        var sep = ReportCsv.Separator;
        var expected = $"Producto{sep}Cantidad{sep}Ingresos{Environment.NewLine}" +
                       $"Refresco{sep}{(3m).ToString("N2", CultureInfo.CurrentCulture)}{sep}{(60m).ToString("N2", CultureInfo.CurrentCulture)}";

        Assert.Equal(expected, csv);
    }

    [Fact]
    public void BuildSummary_IncludesCancelledAndTotals()
    {
        var csv = ReportCsv.BuildSummary(new PeriodSummaryDto(5, 1, 400m, 64m, 464m));

        Assert.Contains("5", csv);
        Assert.Contains("1", csv);
        Assert.Contains((464m).ToString("N2", CultureInfo.CurrentCulture), csv);
    }

    [Fact]
    public void BuildCustomerBalances_EscapesSeparator()
    {
        var csv = ReportCsv.BuildCustomerBalances(new[]
        {
            new CustomerBalanceDto("López; S.A.", "55-1", 10m),
            new CustomerBalanceDto("Dueña \"J\"", null, 5m)
        });

        Assert.Contains("\"López; S.A.\"", csv);
        Assert.Contains("\"Dueña \"\"J\"\"\"", csv);
    }

    [Fact]
    public void BuildDaily_UsesIsoDateAndTotals()
    {
        var csv = ReportCsv.BuildDaily(new[]
        {
            new DailySalesDto(new DateTime(2026, 9, 19), 2, 232m)
        });

        Assert.Contains("2026-09-19", csv);
        Assert.Contains("2", csv);
        Assert.Contains((232m).ToString("N2", CultureInfo.CurrentCulture), csv);
    }
}