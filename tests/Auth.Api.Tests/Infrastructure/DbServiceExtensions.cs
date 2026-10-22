using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Auth.Api.Tests.Infrastructure;

public static class DbServiceExtensions
{
    public static void ReplaceWithSqlite<TContext>(this IServiceCollection services, SqliteConnection connection)
        where TContext : DbContext
    {
        services.RemoveAll<DbContextOptions<TContext>>();
        services.AddDbContext<TContext>(options => options.UseSqlite(connection));
    }

    public static void EnsureDatabase<TContext>(this IServiceProvider provider) where TContext : DbContext
    {
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<TContext>().Database.EnsureCreated();
    }
}
