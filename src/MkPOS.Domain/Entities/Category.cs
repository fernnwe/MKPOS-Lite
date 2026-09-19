namespace MKPOS.Domain.Entities;

/// <summary>
/// Agrupación de productos. El borrado es lógico (<see cref="IsActive"/>)
/// para no romper el historial de productos que ya la referencian.
/// </summary>
public sealed class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}