using RabbitMQ.Client;

namespace F1.Collector.Extensions;

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
            var options = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

            return connection.CreateChannelAsync(options).GetAwaiter().GetResult();
        });

        return services;
    }
}
