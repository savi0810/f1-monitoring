using Alerts.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Extensions;

public static class AlertsExtensions
{
    public static IServiceCollection AddAlertsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
