using RabbitMQ.Client;
using Telemetry.Api.Publishing;

namespace Telemetry.Api.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost"
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

        services.AddKeyedSingleton<IChannel>("imports", CreatePublisherChannel);

        services.AddSingleton(sp =>
            new ImportRequestPublisher(
                sp.GetRequiredKeyedService<IChannel>("imports")));

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
