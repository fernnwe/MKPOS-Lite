namespace MKPOS.Domain.Entities;

/// <summary>
/// Par clave/valor para preferencias y opciones del sistema
/// (por ejemplo, preferencias de impresión o de caja).
/// </summary>
public sealed class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}