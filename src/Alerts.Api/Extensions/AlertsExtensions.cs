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

        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<IAlertRuleService, AlertRuleService>();

        return services;
    }
}
