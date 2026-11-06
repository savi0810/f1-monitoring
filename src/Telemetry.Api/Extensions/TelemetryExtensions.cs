using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Services;

namespace Telemetry.Api.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ILapRepository, LapRepository>();
        services.AddScoped<IMeetingRepository, MeetingRepository>();
        services.AddScoped<ISessionDriverRepository, SessionDriverRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionImportService, SessionImportService>();
        services.AddScoped<ITelemetryQueryService, TelemetryQueryService>();

        return services;
    }
}
