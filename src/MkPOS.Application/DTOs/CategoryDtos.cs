namespace MKPOS.Application.DTOs;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    int ProductCount,
    bool IsActive);

public sealed record CategoryInput(
    string Name,
    string? Description,
    bool IsActive = true);