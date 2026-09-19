using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using MKPOS.Tests.Helpers;

namespace MKPOS.Tests;

public sealed class ProductServiceTests
{
    private const string ValidSku = "P-001";
    private const string ValidName = "Refresco 600 ml";

    private static ProductService CreateSut(
        out FakeProductRepository products,
        out FakeCategoryRepository categories)
    {
        products = new FakeProductRepository();
        categories = new FakeCategoryRepository();
        return new ProductService(products, categories);
    }

    private static ProductInput ValidInput(string? sku = null, string? name = null)
    {
        return new ProductInput(
            Sku: sku ?? ValidSku,
            Name: name ?? ValidName,
            Description: "Bebida",
            Barcode: "7501234567890",
            CategoryId: null,
            PurchasePrice: 10m,
            SalePrice: 15.5m,
            TaxRate: 16m,
            Stock: 50m,
            MinStock: 5m);
    }

    [Fact]
    public async Task CreateAsync_AddsProduct()
    {
        var sut = CreateSut(out var products, out _);

        var result = await sut.CreateAsync(ValidInput());

        Assert.True(result.Success);
        var list = await products.GetAllAsync(null, null, includeInactive: true);
        Assert.Single(list);
        Assert.Equal(ValidSku, list[0].Sku);
    }

    [Fact]
    public async Task CreateAsync_Fails_OnBlankSku()
    {
        var sut = CreateSut(out _, out _);

        var result = await sut.CreateAsync(ValidInput(sku: "  "));

        Assert.False(result.Success);
        Assert.Equal("El código (SKU) es obligatorio.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_Fails_OnDuplicateSku()
    {
        var sut = CreateSut(out var products, out _);
        await sut.CreateAsync(ValidInput());

        var result = await sut.CreateAsync(ValidInput(sku: "p-001"));

        Assert.False(result.Success);
        Assert.Equal("Ya existe un producto con ese código (SKU).", result.Error);
    }

    [Fact]
    public async Task CreateAsync_Fails_OnDuplicateBarcode()
    {
        var sut = CreateSut(out var products, out _);
        await sut.CreateAsync(ValidInput(sku: "P-001", name: "Primero"));

        var result = await sut.CreateAsync(ValidInput(sku: "P-002", name: "Segundo"));

        Assert.False(result.Success);
        Assert.Equal("Ya existe un producto con ese código de barras.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_Fails_WhenCategoryDoesNotExist()
    {
        var sut = CreateSut(out _, out _);

        var result = await sut.CreateAsync(ValidInput() with { CategoryId = Guid.NewGuid() });

        Assert.False(result.Success);
        Assert.Equal("La categoría seleccionada no existe.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ChangesPriceAndKeepsStock()
    {
        var sut = CreateSut(out var products, out _);
        await sut.CreateAsync(ValidInput());
        var product = (await products.GetAllAsync(null, null, true))[0];

        var result = await sut.UpdateAsync(product.Id, ValidInput() with { SalePrice = 18m, Stock = 60m });

        Assert.True(result.Success);
        var updated = await products.GetByIdAsync(product.Id);
        Assert.Equal(18m, updated!.SalePrice);
        Assert.Equal(60m, updated.Stock);
    }

    [Fact]
    public async Task AdjustStockAsync_SetsNewStock()
    {
        var sut = CreateSut(out var products, out _);
        await sut.CreateAsync(ValidInput());
        var product = (await products.GetAllAsync(null, null, true))[0];

        var result = await sut.AdjustStockAsync(product.Id, 25m);

        Assert.True(result.Success);
        var updated = await products.GetByIdAsync(product.Id);
        Assert.Equal(25m, updated!.Stock);
    }

    [Fact]
    public async Task AdjustStockAsync_Fails_ForNegativeStock()
    {
        var sut = CreateSut(out var products, out _);
        await sut.CreateAsync(ValidInput());
        var product = (await products.GetAllAsync(null, null, true))[0];

        var result = await sut.AdjustStockAsync(product.Id, -1m);

        Assert.False(result.Success);
        Assert.Equal("La existencia no puede ser negativa.", result.Error);
    }

    [Fact]
    public async Task ToggleActiveAsync_DeactivatesProduct()
    {
        var sut = CreateSut(out var products, out _);
        await sut.CreateAsync(ValidInput());
        var product = (await products.GetAllAsync(null, null, true))[0];

        var result = await sut.ToggleActiveAsync(product.Id);

        Assert.True(result.Success);
        var updated = await products.GetByIdAsync(product.Id);
        Assert.False(updated!.IsActive);
        Assert.Empty(await products.GetAllAsync(null, null, includeInactive: false));
    }

    [Fact]
    public async Task GetProductsAsync_FiltersBySearch()
    {
        var sut = CreateSut(out _, out _);
        await sut.CreateAsync(ValidInput());
        await sut.CreateAsync(ValidInput(sku: "P-002", name: "Galletas") with { Barcode = null });

        var found = await sut.GetProductsAsync("galletas", null);

        Assert.Single(found);
        Assert.Equal("Galletas", found[0].Name);
    }
}