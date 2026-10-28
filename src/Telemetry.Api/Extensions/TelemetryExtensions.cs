using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data;

namespace Telemetry.Api.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
