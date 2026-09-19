using MKPOS.Domain.Entities;

namespace MKPOS.App.Services;

/// <summary>
/// Mantiene el estado de la sesión activa mientras la aplicación está abierta.
/// </summary>
public sealed class SessionService
{
    public User? CurrentUser { get; private set; }

    public void StartSession(User user)
    {
        CurrentUser = user;
    }

    public void EndSession()
    {
        CurrentUser = null;
    }
}