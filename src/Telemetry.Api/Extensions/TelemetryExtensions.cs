using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data;
using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ILapRepository, LapRepository>();

        return services;
    }
}
