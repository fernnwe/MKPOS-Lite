using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using System.Collections.ObjectModel;
using System.Text;

namespace MKPOS.App.ViewModels;

public partial class ReportsViewModel : ObservableObject, IModuleViewModel
{
    private readonly IReportService _reports;

    private PeriodSummaryDto? _summary;

    public ObservableCollection<DailySalesDto> DailySales { get; } = new();
    public ObservableCollection<PaymentMethodSummaryDto> SalesByPaymentMethod { get; } = new();
    public ObservableCollection<TopProductDto> TopProducts { get; } = new();
    public ObservableCollection<CustomerBalanceDto> CustomerBalances { get; } = new();

    [ObservableProperty]
    private DateTime _fromDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _toDate = DateTime.Today;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int _totalTickets;

    [ObservableProperty]
    private int _cancelledTickets;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _total;

    public ReportsViewModel(IReportService reports)
    {
        _reports = reports;
    }

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (FromDate.Date > ToDate.Date)
        {
            ErrorMessage = "La fecha inicial debe ser anterior o igual a la final.";
            return;
        }

        var from = FromDate.Date;
        var to = ToDate.Date.AddDays(1);

        IsBusy = true;
        try
        {
            _summary = await _reports.GetPeriodSummaryAsync(from, to);
            TotalTickets = _summary.TotalTickets;
            CancelledTickets = _summary.CancelledTickets;
            Subtotal = _summary.Subtotal;
            TaxAmount = _summary.TaxAmount;
            Total = _summary.Total;

            DailySales.Replace(await _reports.GetDailySalesAsync(from, to));
            SalesByPaymentMethod.Replace(await _reports.GetSalesByPaymentMethodAsync(from, to));
            TopProducts.Replace(await _reports.GetTopProductsAsync(from, to, 20));
            CustomerBalances.Replace(await _reports.GetCustomerBalancesAsync());

            ErrorMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ExportSummary()
    {
        if (_summary is not null)
        {
            SaveCsv($"resumen_{Stamp()}.csv", ReportCsv.BuildSummary(_summary));
        }
    }

    [RelayCommand]
    private void ExportDaily()
    {
        SaveCsv($"ventas_por_dia_{Stamp()}.csv", ReportCsv.BuildDaily(DailySales));
    }

    [RelayCommand]
    private void ExportByPayment()
    {
        SaveCsv($"ventas_por_metodo_{Stamp()}.csv", ReportCsv.BuildByPayment(SalesByPaymentMethod));
    }

    [RelayCommand]
    private void ExportTopProducts()
    {
        SaveCsv($"productos_mas_vendidos_{Stamp()}.csv", ReportCsv.BuildTopProducts(TopProducts));
    }

    [RelayCommand]
    private void ExportCustomerBalances()
    {
        SaveCsv($"cartera_clientes_{Stamp()}.csv", ReportCsv.BuildCustomerBalances(CustomerBalances));
    }

    private string Stamp() => DateTime.Now.ToString("yyyyMMdd_HHmmss");

    private void SaveCsv(string defaultFileName, string content)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = defaultFileName,
            DefaultExt = ".csv",
            Filter = "CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, content, new UTF8Encoding(true));
        }
    }
}