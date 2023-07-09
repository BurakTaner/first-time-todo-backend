using TodoBackend.Models;

namespace TodoBackend.Services;

public interface IJwtService
{
    string GenerateJwt(User user);
}
