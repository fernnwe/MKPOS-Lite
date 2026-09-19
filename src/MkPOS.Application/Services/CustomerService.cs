using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Domain.Entities;

namespace MKPOS.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;

    public CustomerService(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(
        string? search,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var list = await _customers.GetAllAsync(search, includeInactive, ct);

        return list
            .Select(c => new CustomerDto(c.Id, c.Name, c.Phone, c.Email, c.Address, c.Balance, c.IsActive))
            .ToList();
    }

    public async Task<OperationResult> CreateAsync(CustomerInput input, CancellationToken ct = default)
    {
        var name = input.Name.Trim();
        if (name.Length == 0)
        {
            return OperationResult.Fail("El nombre del cliente es obligatorio.");
        }

        var email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        if (email is not null && !IsValidEmail(email))
        {
            return OperationResult.Fail("El correo electrónico no tiene un formato válido.");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
            Email = email,
            Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim(),
            Balance = 0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _customers.AddAsync(customer, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> UpdateAsync(Guid id, CustomerInput input, CancellationToken ct = default)
    {
        var customer = await _customers.GetByIdAsync(id, ct);
        if (customer is null)
        {
            return OperationResult.Fail("El cliente no existe.");
        }

        var name = input.Name.Trim();
        if (name.Length == 0)
        {
            return OperationResult.Fail("El nombre del cliente es obligatorio.");
        }

        var email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        if (email is not null && !IsValidEmail(email))
        {
            return OperationResult.Fail("El correo electrónico no tiene un formato válido.");
        }

        customer.Name = name;
        customer.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        customer.Email = email;
        customer.Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim();
        customer.UpdatedAt = DateTime.UtcNow;

        await _customers.UpdateAsync(customer, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _customers.GetByIdAsync(id, ct);
        if (customer is null)
        {
            return OperationResult.Fail("El cliente no existe.");
        }

        if (!customer.IsActive)
        {
            return OperationResult.Ok();
        }

        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;

        await _customers.UpdateAsync(customer, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ApplyPaymentAsync(Guid id, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0)
        {
            return OperationResult.Fail("El abono debe ser mayor a cero.");
        }

        var customer = await _customers.GetByIdAsync(id, ct);
        if (customer is null)
        {
            return OperationResult.Fail("El cliente no existe.");
        }

        customer.Balance = Math.Max(0m, customer.Balance - amount);
        customer.UpdatedAt = DateTime.UtcNow;

        await _customers.UpdateAsync(customer, ct);
        return OperationResult.Ok();
    }

    private static bool IsValidEmail(string email)
    {
        return email.Contains('@');
    }
}