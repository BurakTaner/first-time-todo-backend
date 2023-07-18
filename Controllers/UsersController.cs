using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoBackend.Data;
using TodoBackend.DTO;
using TodoBackend.Models;
using TodoBackend.Services;

namespace TodoBackend.Controllers;

[ApiController]
public class UsersController : ControllerBase
{
    private readonly TodosDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly DateTimeOffset expTime = DateTimeOffset.Now.AddSeconds(30);
    private readonly IJwtService _jwtService;
    public UsersController(TodosDbContext context, ICacheService cacheService, IJwtService jwtService)
    {
        _context = context;
        _cacheService = cacheService;
        _jwtService = jwtService;
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
    {
        User registeredUser = await _context.Users.FirstOrDefaultAsync(a => a.Username == userDTO.Username);
        if (registeredUser is null)
        {
            User user = new User(
                    userDTO.Username,
                    userDTO.Password
                    );
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        return BadRequest("There is already a user with that username");
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
    {
        User cachedUser = _cacheService.Get<User>($"{userDTO.Username}");
        if (cachedUser is not null)
        {
            AuthResponse cachedAuthResponse = await _jwtService.GenerateJwt(cachedUser);
            return Ok(cachedAuthResponse);
        }
        User user = await _context.Users.FirstOrDefaultAsync(a => a.Username == userDTO.Username && a.Password == userDTO.Password);
        if (user is null)
        {
            return NotFound();
        }
        bool isCached = _cacheService.Set<User>($"{user.Username}", user, this.expTime);
        if (!isCached)
            Log.Warning("I tried to cache user information but failed");
        AuthResponse authResponse = await _jwtService.GenerateJwt(user);
        return Ok(authResponse);
    }
    [HttpPost("/refreshToken")]
    public async Task<IActionResult> GetRefreshToken([FromBody] TokenRequestDTO tokenRequestDTO)
    {
        if (ModelState.IsValid)
        {
            AuthResponse authResponse = await _jwtService.GenerateRefreshToken(tokenRequestDTO);
            if (authResponse is null)
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new() { "Invalid token." }
                });
            return Ok(authResponse);
        }
        return BadRequest(new AuthResponse()
        {
            Result = false,
            Errors = new() { "Invalid parameters." }
        });
    }
}
