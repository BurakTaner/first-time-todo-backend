using TodoBackend.DTO;
using TodoBackend.Models;

namespace TodoBackend.Services;

public interface IJwtService
{
    Task<AuthResponse> GenerateJwt(User user);
    Task<AuthResponse> GenerateRefreshToken(TokenRequestDTO tokenRequestDTO);
}
