using Auth.Api.Data.Models;

namespace Auth.Api.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
