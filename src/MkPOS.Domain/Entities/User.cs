namespace MKPOS.Domain.Entities;

/// <summary>
/// Usuario del sistema. En esta fase solo se contemplan administradores,
/// pero el modelo ya permite diferenciar roles y estado de cuenta.
/// </summary>
public sealed class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}