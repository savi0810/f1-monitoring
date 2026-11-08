using F1.Collector.Publishing;
using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace F1.Collector.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            UserName = configuration["RabbitMq:UserName"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest"
        });

        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<ConnectionFactory>();

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();

            return connection.CreateChannelAsync().GetAwaiter().GetResult();
        });

        services.AddKeyedSingleton<IChannel>("events", CreatePublisherChannel);
        services.AddKeyedSingleton<IChannel>("catalog", CreatePublisherChannel);
        services.AddKeyedSingleton<IChannel>("live-snapshot", CreatePublisherChannel);

        services.AddSingleton(sp =>
            new EventPublisher(sp.GetRequiredKeyedService<IChannel>("events")));

        services.AddKeyedSingleton<EventPublisher>("catalog", (sp, _) =>
            new EventPublisher(sp.GetRequiredKeyedService<IChannel>("catalog")));

        services.AddSingleton(sp =>
            new LiveSnapshotPublisher(sp.GetRequiredKeyedService<IChannel>("live-snapshot")));

        return services;
    }

    private static IChannel CreatePublisherChannel(IServiceProvider services, object? _)
    {
        var connection = services.GetRequiredService<IConnection>();
        var options = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        return connection.CreateChannelAsync(options).GetAwaiter().GetResult();
    }
}
