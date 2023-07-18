using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoBackend.Configuration;
using TodoBackend.Data;
using TodoBackend.DTO;
using TodoBackend.Models;
using TodoBackend.Utils;

namespace TodoBackend.Services;

public class JwtService : IJwtService
{
    private readonly TodosDbContext _context;
    private readonly JwtConfig _jwtConfig;
    private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    private readonly TokenValidationParameters _tokenValidationParameters;
    public JwtService(TodosDbContext context, IOptionsMonitor<JwtConfig> _optionsMonitor, TokenValidationParameters tokenValidationParameters)
    {
        _context = context;
        _jwtConfig = _optionsMonitor.CurrentValue;
        _tokenValidationParameters = tokenValidationParameters;
    }

    public async Task<AuthResponse> GenerateJwt(User user)
    {
        byte[] key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[] {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Username),
                    new Claim("Password", user.Password)
                    }),
            Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTime),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512)
        };
        SecurityToken token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        string jwtToken = jwtSecurityTokenHandler.WriteToken(token);

        RefreshToken refreshToken = new()
        {
            Token = RandomStringGenerator.GenerateRandomString(32),
            JwtId = token.Id,
            ExpiryTime = DateTime.UtcNow.AddMonths(6),
            IsUsed = false,
            IsRevoked = false,
            UserId = user.Id,
            AddedDate = DateTime.UtcNow
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new()
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token,
            Result = true
        };
    }

    public async Task<AuthResponse> GenerateRefreshToken(TokenRequestDTO tokenRequestDTO)
    {
        try
        {
            _tokenValidationParameters.ValidateLifetime = false;
            ClaimsPrincipal claims = jwtSecurityTokenHandler.ValidateToken(tokenRequestDTO.Jwt, _tokenValidationParameters, out SecurityToken validatedToken);
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                bool result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                if (!result)
                    return null;
            }
            long expiryDateUtc = long.Parse(claims.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Exp).Value);
            DateTime dateTime = UnixToDateTime.ReturnDateTime(expiryDateUtc);
            if (dateTime < DateTime.Now)
            {
                return new()
                {
                    Result = false,
                    Errors = new() { "Token expires" }
                };
            }
            RefreshToken refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(a => a.Token == tokenRequestDTO.RefreshToken);
            if (refreshToken is null)
            {
                return new()
                {
                    Result = false,
                    Errors = new() { "Invalid token" }
                };
            }
            if (refreshToken.IsUsed)
                return new()
                {
                    Result = false,
                    Errors = new() { "Invalid token" }
                };

            if (refreshToken.IsRevoked)
                return new()
                {
                    Result = false,
                    Errors = new() { "Invalid token" }
                };
            string jti = claims.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Jti).Value;
            if (jti != refreshToken.JwtId)
            {
                return new()
                {
                    Result = false,
                    Errors = new() { "Invalid token" }
                };
            }
            refreshToken.IsUsed = true;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
            User user = await _context.Users.FirstOrDefaultAsync(a => a.Id == refreshToken.UserId);
            return await this.GenerateJwt(user);
        }
        catch (Exception ex)
        {
            return new()
            {
                Result = false,
                Errors = new() { "Server error, please try again." }
            };
        }
    }
}
