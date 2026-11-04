using Live.Api.Data.Repositories;
using StackExchange.Redis;

namespace Live.Api.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisOptions = ConfigurationOptions.Parse(
            configuration.GetConnectionString("Redis") ?? "localhost:6379");
        redisOptions.Password = configuration["Redis:Password"];

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions));

        services.AddScoped<ILiveSnapshotRepository, LiveSnapshotRepository>();
        services.AddScoped<ILiveSourceStateRepository, LiveSourceStateRepository>();

        return services;
    }
}
