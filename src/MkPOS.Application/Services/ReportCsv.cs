using System.Globalization;
using System.Text;
using MKPOS.Application.DTOs;

namespace MKPOS.Application.Services;

/// <summary>
/// Serializa los reportes a CSV con separador ';' y formato de moneda local,
/// listo para abrir en una hoja de cálculo.
/// </summary>
public static class ReportCsv
{
    public const string Separator = ";";

    public static string BuildSummary(PeriodSummaryDto summary)
    {
        var rows = new List<string[]>
        {
            new[] { "Tickets", "Canceladas", "Subtotal", "IVA", "Total" },
            new[]
            {
                summary.TotalTickets.ToString(CultureInfo.CurrentCulture),
                summary.CancelledTickets.ToString(CultureInfo.CurrentCulture),
                summary.Subtotal.ToString("N2", CultureInfo.CurrentCulture),
                summary.TaxAmount.ToString("N2", CultureInfo.CurrentCulture),
                summary.Total.ToString("N2", CultureInfo.CurrentCulture)
            }
        };

        return Build(rows);
    }

    public static string BuildDaily(IEnumerable<DailySalesDto> daily)
    {
        var rows = new List<string[]> { new[] { "Fecha", "Tickets", "Total" } };
        rows.AddRange(daily.Select(d => new[]
        {
            d.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            d.Tickets.ToString(CultureInfo.CurrentCulture),
            d.Total.ToString("N2", CultureInfo.CurrentCulture)
        }));
        return Build(rows);
    }

    public static string BuildByPayment(IEnumerable<PaymentMethodSummaryDto> payments)
    {
        var rows = new List<string[]> { new[] { "Método de pago", "Tickets", "Total" } };
        rows.AddRange(payments.Select(p => new[]
        {
            p.PaymentMethodName,
            p.Tickets.ToString(CultureInfo.CurrentCulture),
            p.Total.ToString("N2", CultureInfo.CurrentCulture)
        }));
        return Build(rows);
    }

    public static string BuildTopProducts(IEnumerable<TopProductDto> products)
    {
        var rows = new List<string[]> { new[] { "Producto", "Cantidad", "Ingresos" } };
        rows.AddRange(products.Select(p => new[]
        {
            p.ProductName,
            p.Quantity.ToString("N2", CultureInfo.CurrentCulture),
            p.Revenue.ToString("N2", CultureInfo.CurrentCulture)
        }));
        return Build(rows);
    }

    public static string BuildCustomerBalances(IEnumerable<CustomerBalanceDto> balances)
    {
        var rows = new List<string[]> { new[] { "Cliente", "Teléfono", "Saldo" } };
        rows.AddRange(balances.Select(b => new[]
        {
            b.CustomerName,
            b.Phone ?? string.Empty,
            b.Balance.ToString("N2", CultureInfo.CurrentCulture)
        }));
        return Build(rows);
    }

    private static string Build(IEnumerable<string[]> rows)
    {
        var builder = new StringBuilder();
        bool firstLine = true;

        foreach (var row in rows)
        {
            if (!firstLine)
            {
                builder.AppendLine();
            }

            var cells = row.Select(Escape);
            builder.Append(string.Join(Separator, cells));
            firstLine = false;
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (value.Contains(Separator) || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}