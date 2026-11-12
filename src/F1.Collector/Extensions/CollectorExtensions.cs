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
            .Validate(options => options.PollIntervalMilliseconds > 0, "Espn:PollIntervalMilliseconds must be positive.")
            .Validate(options => options.SchedulePollIntervalSeconds > 0, "Espn:SchedulePollIntervalSeconds must be positive.")
            .Validate(options => options.RequestTimeoutSeconds > 0, "Espn:RequestTimeoutSeconds must be positive.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.EventId), "Espn:EventId is required when ESPN is enabled.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.CompetitionId), "Espn:CompetitionId is required when ESPN is enabled.")
            .Validate(options => options.FastPollingLeadMinutes >= 0, "Espn:FastPollingLeadMinutes must not be negative.")
            .Validate(options => options.PostFinishMinutes >= 0, "Espn:PostFinishMinutes must not be negative.")
            .ValidateOnStart();

        services.AddSingleton<EspnSnapshotMapper>();
        services.AddSingleton<EspnPollingSchedule>();

        services.AddHttpClient<OpenF1Client>(client =>
        {
            client.BaseAddress = new Uri("https://api.openf1.org/v1/");
        }).AddStandardResilienceHandler();

        services.AddHttpClient<EspnClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Espn:BaseUrl"] ?? "https://site.api.espn.com/apis/site/v2/sports/racing/f1/");
        });

        services.AddHostedService<ImportRequestedConsumer>();
        services.AddHostedService<SessionCatalogPublisher>();
        services.AddHostedService<EspnPollingWorker>();

        return services;
    }
}
