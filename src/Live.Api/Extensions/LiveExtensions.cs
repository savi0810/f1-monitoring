using Live.Api.Services;

namespace Live.Api.Extensions;

public static class LiveExtensions
{
    public static IServiceCollection AddLiveServices(this IServiceCollection services)
    {
        services.AddScoped<TimingService>();
        services.AddScoped<LiveSourceService>();

        return services;
    }
}
