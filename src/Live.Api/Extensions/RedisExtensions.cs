using StackExchange.Redis;

namespace Live.Api.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            var redisOptions = ConfigurationOptions.Parse(
                configuration.GetConnectionString("Redis") ?? "localhost:6379");
            redisOptions.Password = configuration["Redis:Password"];
            options.ConfigurationOptions = redisOptions;
        });

        return services;
    }
}
