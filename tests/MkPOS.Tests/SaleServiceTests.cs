using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using MKPOS.Domain.Entities;
using MKPOS.Tests.Helpers;

namespace MKPOS.Tests;

public sealed class SaleServiceTests
{
    private static SaleService CreateSut(
        out FakeSaleRepository sales,
        out FakeProductRepository products,
        out FakeCompanyRepository companies,
        out FakeCustomerRepository customers)
    {
        sales = new FakeSaleRepository();
        products = new FakeProductRepository();
        companies = new FakeCompanyRepository();
        customers = new FakeCustomerRepository();
        return new SaleService(sales, products, companies, customers);
    }

    private static Customer NewCustomer(string name = "Cliente Uno", decimal balance = 0m)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Balance = balance,
            IsActive = true
        };
    }

    private static Product NewProduct(
        string sku = "P-001",
        string? name = null,
        decimal salePrice = 15.5m,
        decimal? taxRate = 16m,
        decimal stock = 50m)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Name = name ?? sku,
            SalePrice = salePrice,
            TaxRate = taxRate,
            Stock = stock,
            MinStock = 5m,
            IsActive = true
        };
    }

    private static CreateSaleRequest Request(Product product, decimal quantity = 1, PaymentMethod method = PaymentMethod.Cash, decimal? paymentAmount = null, Guid? customerId = null)
    {
        return new CreateSaleRequest
        {
            Lines = new List<SaleLineInput> { new(product.Id, quantity) },
            PaymentMethod = method,
            PaymentAmount = paymentAmount ?? 0m,
            CustomerId = customerId
        };
    }

    [Fact]
    public async Task CompleteAsync_WithCash_ComputesTotalsFolioAndChange()
    {
        var sut = CreateSut(out _, out var products, out _, out _);
        var product = NewProduct(stock: 50m);
        await products.AddAsync(product);

        var result = await sut.CompleteAsync(Request(product, quantity: 2, paymentAmount: 40m), userId: Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(1, result.TicketNumber);
        Assert.Equal(35.96m, result.Total);          // 31.00 + 4.96
        Assert.Equal(4.04m, result.ChangeAmount);    // 40.00 - 35.96
        Assert.Equal(48m, product.Stock);            // 50 - 2
    }

    [Fact]
    public async Task CompleteAsync_WithCard_PaymentAmountEqualsTotal()
    {
        var sut = CreateSut(out _, out var products, out _, out _);
        var product = NewProduct(salePrice: 100m, taxRate: 0m);
        await products.AddAsync(product);

        var result = await sut.CompleteAsync(Request(product, method: PaymentMethod.Card, paymentAmount: null), userId: Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(0m, result.ChangeAmount);
        Assert.Equal(100m, result.Total);
    }

    [Fact]
    public async Task CompleteAsync_EmptyCart_Fails()
    {
        var sut = CreateSut(out _, out _, out _, out _);

        var request = new CreateSaleRequest
        {
            Lines = new List<SaleLineInput>(),
            PaymentMethod = PaymentMethod.Cash,
            PaymentAmount = 0m
        };
        var result = await sut.CompleteAsync(request, userId: Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("El carrito está vacío.", result.Error);
    }

    [Fact]
    public async Task CompleteAsync_InsufficientCash_Fails()
    {
        var sut = CreateSut(out _, out var products, out _, out _);
        var product = NewProduct(salePrice: 100m, taxRate: 0m);
        await products.AddAsync(product);

        var result = await sut.CompleteAsync(Request(product, paymentAmount: 50m), userId: Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("El efectivo recibido es insuficiente para cubrir la venta.", result.Error);
        Assert.Equal(50m, product.Stock); // no se toca el stock
    }

    [Fact]
    public async Task CompleteAsync_OutOfStock_Fails()
    {
        var sut = CreateSut(out _, out var products, out _, out _);
        var product = NewProduct(stock: 1m);
        await products.AddAsync(product);

        var result = await sut.CompleteAsync(Request(product, quantity: 3), userId: Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("Sin existencias suficientes", result.Error);
    }

    [Fact]
    public async Task CompleteAsync_UsesCompanyTax_WhenProductHasNoOwn()
    {
        var sut = CreateSut(out _, out var products, out var companies, out _);
        var product = NewProduct(salePrice: 100m, taxRate: null);
        await products.AddAsync(product);
        await companies.SaveAsync(new Company { Name = "Test", TaxRate = 10m });

        var result = await sut.CompleteAsync(Request(product, paymentAmount: 200m), userId: Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(110m, result.Total);
        Assert.Equal(90m, result.ChangeAmount); // 200 - 110
    }

    [Fact]
    public async Task CompleteAsync_AssignsSequentialFolio()
    {
        var sut = CreateSut(out _, out var products, out _, out _);
        var product = NewProduct(salePrice: 10m, taxRate: 0m);
        await products.AddAsync(product);
        var userId = Guid.NewGuid();

        var first = await sut.CompleteAsync(Request(product, paymentAmount: 50m), userId);
        var second = await sut.CompleteAsync(Request(product, paymentAmount: 50m), userId);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(1, first.TicketNumber);
        Assert.Equal(2, second.TicketNumber);
    }

    [Fact]
    public async Task CompleteAsync_WithCredit_IncreasesBalanceAndSnapshotsCustomer()
    {
        var sut = CreateSut(out var sales, out var products, out _, out var customers);
        var product = NewProduct(salePrice: 100m, taxRate: 0m);
        await products.AddAsync(product);
        var customer = NewCustomer(name: "Cliente Fiado");
        await customers.AddAsync(customer);

        var result = await sut.CompleteAsync(Request(product, quantity: 2, method: PaymentMethod.Credit, customerId: customer.Id), userId: Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(200m, result.Total);
        Assert.Equal(0m, result.ChangeAmount);
        Assert.Equal(200m, customer.Balance);
        Assert.Equal(48m, product.Stock);

        var recent = await sales.GetRecentAsync(1);
        var sale = Assert.Single(recent);
        Assert.Equal("Cliente Fiado", sale.CustomerName);
        Assert.Equal(customer.Id, sale.CustomerId);
    }

    [Fact]
    public async Task CompleteAsync_WithCredit_RequiresCustomerId()
    {
        var sut = CreateSut(out _, out var products, out _, out _);
        var product = NewProduct(salePrice: 100m, taxRate: 0m);
        await products.AddAsync(product);

        var result = await sut.CompleteAsync(Request(product, method: PaymentMethod.Credit), userId: Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Seleccione un cliente para la venta a crédito.", result.Error);
    }

    [Fact]
    public async Task CompleteAsync_WithCredit_InactiveCustomerFails()
    {
        var sut = CreateSut(out _, out var products, out _, out var customers);
        var product = NewProduct(salePrice: 100m, taxRate: 0m);
        await products.AddAsync(product);
        var customer = NewCustomer();
        customer.IsActive = false;
        await customers.AddAsync(customer);

        var result = await sut.CompleteAsync(Request(product, method: PaymentMethod.Credit, customerId: customer.Id), userId: Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("El cliente seleccionado no es válido.", result.Error);
        Assert.Equal(0m, customer.Balance);
    }
}