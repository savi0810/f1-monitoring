using RabbitMQ.Client;

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

        return services;
    }
}
