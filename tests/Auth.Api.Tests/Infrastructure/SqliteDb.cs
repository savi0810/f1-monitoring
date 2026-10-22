using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Tests.Infrastructure;

public sealed class SqliteDb<TContext> where TContext : DbContext
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TContext> _options;

    public SqliteDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<TContext>().UseSqlite(_connection).Options;
    }

    public TContext Create() => (TContext)Activator.CreateInstance(typeof(TContext), _options)!;
}
