using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MKPOS.Application.Abstractions;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Infrastructure.Persistence;
using MKPOS.Infrastructure.Persistence.Repositories;
using MKPOS.Infrastructure.Security;

namespace MKPOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<MKPOSDbContext>(options =>
            options.UseSqlite($"Data Source={AppPaths.DatabasePath}"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}