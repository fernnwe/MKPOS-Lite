using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Domain.Entities;

namespace MKPOS.Application.Services;

public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _sales;
    private readonly IProductRepository _products;
    private readonly ICompanyRepository _companies;
    private readonly ICustomerRepository _customers;

    public SaleService(
        ISaleRepository sales,
        IProductRepository products,
        ICompanyRepository companies,
        ICustomerRepository customers)
    {
        _sales = sales;
        _products = products;
        _companies = companies;
        _customers = customers;
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

        Customer? customer = null;

        if (request.PaymentMethod == PaymentMethod.Cash && request.PaymentAmount < total)
        {
            return CreateSaleResult.Fail("El efectivo recibido es insuficiente para cubrir la venta.");
        }

        if (request.PaymentMethod == PaymentMethod.Credit)
        {
            if (request.CustomerId is not { } customerId)
            {
                return CreateSaleResult.Fail("Seleccione un cliente para la venta a crédito.");
            }

            customer = await _customers.GetByIdAsync(customerId, ct);
            if (customer is null || !customer.IsActive)
            {
                return CreateSaleResult.Fail("El cliente seleccionado no es válido.");
            }
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

        if (customer is not null)
        {
            customer.Balance += total;
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
            CustomerId = customer?.Id,
            CustomerName = customer?.Name,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };

        await _sales.CompleteAsync(sale, adjustedProducts, customer, ct);

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
                s.Items.Count,
                s.CustomerName ?? "Mostrador"))
            .ToList();
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