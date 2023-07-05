using TodoBackend.Models;

namespace TodoBackend.Services;

public interface IJwtService
{
    Task<string> GenerateJwt(User user);
}
