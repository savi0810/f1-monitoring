using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Auth.Api.Tests.Infrastructure;

public class AuthApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public AuthApiFactory()
    {
        _connection.Open();
    }
}
