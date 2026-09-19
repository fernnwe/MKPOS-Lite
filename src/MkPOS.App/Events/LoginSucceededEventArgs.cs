using MKPOS.Application.DTOs;

namespace MKPOS.App.Events;

public sealed class LoginSucceededEventArgs : EventArgs
{
    public AuthResult Result { get; }

    public LoginSucceededEventArgs(AuthResult result)
    {
        Result = result;
    }
}