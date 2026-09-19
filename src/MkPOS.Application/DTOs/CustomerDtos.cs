namespace MKPOS.Application.DTOs;

public sealed record CustomerInput(
    string Name,
    string? Phone,
    string? Email,
    string? Address);

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    decimal Balance,
    bool IsActive);