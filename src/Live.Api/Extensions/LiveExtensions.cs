using Live.Api.Services;

namespace Live.Api.Extensions;

public static class LiveExtensions
{
    public static IServiceCollection AddLiveServices(this IServiceCollection services)
    {
        services.AddSingleton<PositionChangeDetector>();
        services.AddSingleton<RaceCompletionDetector>();
        services.AddScoped<TimingService>();
        services.AddScoped<LiveSourceService>();

        return services;
    }
}
