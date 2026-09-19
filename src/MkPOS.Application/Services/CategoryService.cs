using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Domain.Entities;

namespace MKPOS.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var list = await _categories.GetAllAsync(includeInactive, ct);
        var counts = await _categories.GetProductCountsAsync(ct);

        return list
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description,
                counts.GetValueOrDefault(c.Id), c.IsActive))
            .ToList();
    }

    public async Task<OperationResult> CreateAsync(CategoryInput input, CancellationToken ct = default)
    {
        var name = input.Name.Trim();
        if (name.Length == 0)
        {
            return OperationResult.Fail("El nombre de la categoría es obligatorio.");
        }

        if (await _categories.ExistsNameAsync(name, null, ct))
        {
            return OperationResult.Fail("Ya existe una categoría con ese nombre.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _categories.AddAsync(category, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> UpdateAsync(Guid id, CategoryInput input, CancellationToken ct = default)
    {
        var category = await _categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return OperationResult.Fail("La categoría no existe.");
        }

        var name = input.Name.Trim();
        if (name.Length == 0)
        {
            return OperationResult.Fail("El nombre de la categoría es obligatorio.");
        }

        if (await _categories.ExistsNameAsync(name, id, ct))
        {
            return OperationResult.Fail("Ya existe una categoría con ese nombre.");
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        category.IsActive = input.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _categories.UpdateAsync(category, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return OperationResult.Fail("La categoría no existe.");
        }

        if (!category.IsActive)
        {
            return OperationResult.Ok();
        }

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _categories.UpdateAsync(category, ct);
        return OperationResult.Ok();
    }
}