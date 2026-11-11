using Alerts.Api.Data;
using Alerts.Api.Data.Repositories;
using Alerts.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Extensions;

public static class AlertsExtensions
{
    public static IServiceCollection AddAlertsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDriverSubscriptionRepository, DriverSubscriptionRepository>();
        services.AddScoped<ILiveEventRepository, LiveEventRepository>();
        services.AddScoped<IDriverSubscriptionService, DriverSubscriptionService>();
        services.AddScoped<IPositionAlertService, PositionAlertService>();
        services.AddScoped<IRaceCompletionService, RaceCompletionService>();

        return services;
    }
}
