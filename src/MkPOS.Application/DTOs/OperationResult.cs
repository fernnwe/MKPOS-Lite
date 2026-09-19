namespace MKPOS.Application.DTOs;

/// <summary>
/// Resultado genérico de una operación de negocio.
/// </summary>
public sealed class OperationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static OperationResult Ok() => new() { Success = true };
    public static OperationResult Fail(string error) => new() { Error = error };
}