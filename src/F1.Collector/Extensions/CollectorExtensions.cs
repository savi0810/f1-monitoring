using F1.Collector.Consuming;
using F1.Collector.Espn;
using F1.Collector.Mapping;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;

namespace F1.Collector.Extensions;

public static class CollectorExtensions
{
    public static IServiceCollection AddCollectorServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<OpenF1RateLimiter>();

        services.AddOptions<EspnOptions>()
            .Bind(configuration.GetSection("Espn"))
            .Validate(options => new[] { "Auto", "Start", "Stop" }.Contains(options.Mode, StringComparer.OrdinalIgnoreCase), "Espn:Mode must be Auto, Start, or Stop.")
            .ValidateOnStart();

        services.AddSingleton<EspnSnapshotMapper>();
        services.AddSingleton<EspnPollingSchedule>();

        services.AddHttpClient<OpenF1Client>(client =>
        {
            client.BaseAddress = new Uri("https://api.openf1.org/v1/");
        }).AddStandardResilienceHandler();

        services.AddHttpClient<EspnClient>(client =>
        {
            client.BaseAddress = new Uri("https://site.api.espn.com/apis/site/v2/sports/racing/f1/");
        });

        services.AddHostedService<ImportRequestedConsumer>();
        services.AddHostedService<SessionCatalogPublisher>();
        services.AddHostedService<EspnPollingWorker>();

        return services;
    }
}
