using Microsoft.Extensions.DependencyInjection;
using MKPOS.Application.Services;

namespace MKPOS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}