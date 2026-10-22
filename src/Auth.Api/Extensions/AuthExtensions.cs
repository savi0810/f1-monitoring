using Auth.Api.Data;
using Auth.Api.Data.Repositories;
using Auth.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
