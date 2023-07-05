using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoBackend.Configuration;
using TodoBackend.Data;
using TodoBackend.Models;

namespace TodoBackend.Services;

public class JwtService : IJwtService
{
    private readonly TodosDbContext _context;
    private readonly JwtConfig _jwtConfig;
    public JwtService(TodosDbContext context, IOptionsMonitor<JwtConfig> _optionsMonitor)
    {
        _context = context;
        _jwtConfig = _optionsMonitor.CurrentValue;
    }

    public async Task<string> GenerateJwt(User user)
    {
        byte[] key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);
        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[] {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Username),
                    new Claim("Password", user.Password)
                    }),
            Expires = DateTime.Now.AddMinutes(5),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512)
        };
        SecurityToken token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        string jwtToken = jwtSecurityTokenHandler.WriteToken(token);
        return jwtToken;
    }
}
