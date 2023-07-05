using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
                EntityEntry<User> result = await _context.Users.AddAsync(user);
                string jwtToken = await _jwtService.GenerateJwt(result.Entity);
                await _context.SaveChangesAsync();
                return Created("/login", result);
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
            string CjwtToken = await _jwtService.GenerateJwt(cachedUser);
            return Ok(CjwtToken);
        }
        User user = await _context.Users.FirstOrDefaultAsync(a => a.Username == userDTO.Username && a.Password == userDTO.Password);
        if (user is null)
        {
            return NotFound();
        }
        bool isCached = _cacheService.Set<User>($"{user.Username}", user, this.expTime);
        if (!isCached)
            Log.Warning("I tried to cache user information but failed");
        string jwtToken = await _jwtService.GenerateJwt(user);
        return Ok(jwtToken);
    }
}
