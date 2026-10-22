using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Tests.Infrastructure;

public sealed class SqliteDb<TContext> where TContext : DbContext
{
    private readonly SqliteConnection _connection;

    public SqliteDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }
}
