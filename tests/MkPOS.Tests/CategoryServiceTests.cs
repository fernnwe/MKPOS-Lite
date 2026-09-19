using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using MKPOS.Tests.Helpers;

namespace MKPOS.Tests;

public sealed class CategoryServiceTests
{
    private static CategoryService CreateSut(out FakeCategoryRepository repository)
    {
        repository = new FakeCategoryRepository();
        return new CategoryService(repository);
    }

    [Fact]
    public async Task CreateAsync_AddsActiveCategory()
    {
        var sut = CreateSut(out var repository);

        var result = await sut.CreateAsync(new CategoryInput("Bebidas", "Refrescos y jugos"));

        Assert.True(result.Success);
        var list = await repository.GetAllAsync(includeInactive: false);
        Assert.Single(list);
        Assert.Equal("Bebidas", list[0].Name);
        Assert.True(list[0].IsActive);
    }

    [Fact]
    public async Task CreateAsync_Fails_OnBlankName()
    {
        var sut = CreateSut(out _);

        var result = await sut.CreateAsync(new CategoryInput("  ", null));

        Assert.False(result.Success);
        Assert.Equal("El nombre de la categoría es obligatorio.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_Fails_OnDuplicateName()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(new CategoryInput("Bebidas", null));

        var result = await sut.CreateAsync(new CategoryInput("bebidas", null));

        Assert.False(result.Success);
        Assert.Equal("Ya existe una categoría con ese nombre.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ChangesName()
    {
        var sut = CreateSut(out var repository);
        await sut.CreateAsync(new CategoryInput("Bebidas", null));
        var category = (await repository.GetAllAsync(false))[0];

        var result = await sut.UpdateAsync(category.Id, new CategoryInput("Abarrotes", "Todo"));

        Assert.True(result.Success);
        var updated = await repository.GetByIdAsync(category.Id);
        Assert.Equal("Abarrotes", updated!.Name);
    }

    [Fact]
    public async Task UpdateAsync_Fails_WhenNameAlreadyUsed()
    {
        var sut = CreateSut(out var repository);
        await sut.CreateAsync(new CategoryInput("Bebidas", null));
        await sut.CreateAsync(new CategoryInput("Abarrotes", null));
        var all = await repository.GetAllAsync(false);
        var bebidas = all.First(c => c.Name == "Bebidas");
        var abarrotes = all.First(c => c.Name == "Abarrotes");

        var result = await sut.UpdateAsync(abarrotes.Id, new CategoryInput("Bebidas", null));

        Assert.False(result.Success);
        Assert.Equal("Ya existe una categoría con ese nombre.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_MarksAsInactive()
    {
        var sut = CreateSut(out var repository);
        await sut.CreateAsync(new CategoryInput("Bebidas", null));
        var category = (await repository.GetAllAsync(false))[0];

        await sut.DeleteAsync(category.Id);

        var deactivated = await repository.GetByIdAsync(category.Id);
        Assert.False(deactivated!.IsActive);
        Assert.Empty(await repository.GetAllAsync(includeInactive: false));
    }
}