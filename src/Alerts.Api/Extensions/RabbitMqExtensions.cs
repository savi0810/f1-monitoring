using RabbitMQ.Client;

namespace Alerts.Api.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(_ => new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost"
        });

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<ConnectionFactory>();

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton<IChannel>(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();

            return connection.CreateChannelAsync().GetAwaiter().GetResult();
        });

        services.AddKeyedSingleton<IChannel>("alerts-publisher", CreatePublisherChannel);

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
