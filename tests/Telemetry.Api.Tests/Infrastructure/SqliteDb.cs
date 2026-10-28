using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Telemetry.Api.Tests.Infrastructure;

public sealed class SqliteDb<TContext> : IDisposable where TContext : DbContext
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TContext> _options;

    public SqliteDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<TContext>().UseSqlite(_connection).Options;
        using var context = Create();
        context.Database.EnsureCreated();
    }

    public DbContextOptions<TContext> Options => _options;

    public TContext Create() => (TContext)Activator.CreateInstance(typeof(TContext), _options)!;

    public void Dispose() => _connection.Dispose();
}
