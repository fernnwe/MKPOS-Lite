using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Domain.Entities;

namespace MKPOS.Application.Services;

public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _sales;
    private readonly IProductRepository _products;
    private readonly ICompanyRepository _companies;

    public SaleService(
        ISaleRepository sales,
        IProductRepository products,
        ICompanyRepository companies)
    {
        _sales = sales;
        _products = products;
        _companies = companies;
    }

    public async Task<CreateSaleResult> CompleteAsync(CreateSaleRequest request, Guid userId, CancellationToken ct = default)
    {
        if (request.Lines is null || request.Lines.Count == 0)
        {
            return CreateSaleResult.Fail("El carrito está vacío.");
        }

        var linesByProduct = request.Lines
            .Where(l => l.Quantity > 0)
            .GroupBy(l => l.ProductId)
            .ToList();

        if (linesByProduct.Count == 0)
        {
            return CreateSaleResult.Fail("El carrito está vacío.");
        }

        var productIds = linesByProduct.Select(g => g.Key).ToList();
        var productsById = (await _products.GetByIdsAsync(productIds, ct))
            .ToDictionary(p => p.Id);

        var defaultTaxRate = (await _companies.GetCurrentAsync(ct))?.TaxRate ?? 0m;

        var items = new List<SaleItem>();
        var subtotal = 0m;
        var taxAmount = 0m;

        foreach (var group in linesByProduct)
        {
            if (!productsById.TryGetValue(group.Key, out var product))
            {
                return CreateSaleResult.Fail("Uno de los productos ya no existe.");
            }

            if (!product.IsActive)
            {
                return CreateSaleResult.Fail($"El producto \"{product.Name}\" está inactivo.");
            }

            var quantity = group.Sum(l => l.Quantity);
            if (quantity <= 0)
            {
                return CreateSaleResult.Fail("La cantidad de un producto no es válida.");
            }

            if (product.Stock < quantity)
            {
                return CreateSaleResult.Fail($"Sin existencias suficientes de \"{product.Name}\".");
            }

            var taxRate = product.TaxRate ?? defaultTaxRate;
            var lineTotal = quantity * product.SalePrice;
            subtotal += lineTotal;
            taxAmount += lineTotal * taxRate / 100m;

            items.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.SalePrice,
                TaxRate = taxRate,
                Quantity = quantity,
                LineTotal = lineTotal
            });
        }

        var total = subtotal + taxAmount;

        if (request.PaymentMethod == PaymentMethod.Cash && request.PaymentAmount < total)
        {
            return CreateSaleResult.Fail("El efectivo recibido es insuficiente para cubrir la venta.");
        }

        var change = request.PaymentMethod == PaymentMethod.Cash
            ? request.PaymentAmount - total
            : 0m;

        var adjustedProducts = new List<Product>();
        foreach (var group in linesByProduct)
        {
            var product = productsById[group.Key];
            product.Stock -= group.Sum(l => l.Quantity);
            adjustedProducts.Add(product);
        }

        var ticketNumber = (await _sales.GetLastTicketNumberAsync(ct)) + 1;

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            UserId = userId,
            SaleDate = DateTime.UtcNow,
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            Total = total,
            PaymentMethod = request.PaymentMethod,
            PaymentAmount = request.PaymentMethod == PaymentMethod.Cash ? request.PaymentAmount : total,
            ChangeAmount = change,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };

        await _sales.CompleteAsync(sale, adjustedProducts, ct);

        return CreateSaleResult.Ok(ticketNumber, total, change);
    }

    public async Task<IReadOnlyList<SaleDto>> GetRecentAsync(int take, CancellationToken ct = default)
    {
        var sales = await _sales.GetRecentAsync(take, ct);

        return sales
            .Select(s => new SaleDto(
                s.Id,
                s.TicketNumber,
                s.SaleDate,
                s.Subtotal,
                s.TaxAmount,
                s.Total,
                PaymentMethodName(s.PaymentMethod),
                s.IsCancelled,
                s.Items.Count))
            .ToList();
    }

    private static string PaymentMethodName(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => "Efectivo",
            PaymentMethod.Card => "Tarjeta",
            PaymentMethod.Transfer => "Transferencia",
            _ => method.ToString()
        };
    }
}