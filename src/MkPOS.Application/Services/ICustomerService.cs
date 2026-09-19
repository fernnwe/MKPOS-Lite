using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;

namespace MKPOS.Application.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(string? search, bool includeInactive = false, CancellationToken ct = default);
    Task<OperationResult> CreateAsync(CustomerInput input, CancellationToken ct = default);
    Task<OperationResult> UpdateAsync(Guid id, CustomerInput input, CancellationToken ct = default);
    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken ct = default);
    Task<OperationResult> ApplyPaymentAsync(Guid id, decimal amount, CancellationToken ct = default);
}