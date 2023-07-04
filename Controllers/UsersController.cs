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
    public UsersController(TodosDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
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

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }
        return BadRequest();
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
    {
        User cachedUser = _cacheService.Get<User>($"{userDTO.Username}");
        if (cachedUser is not null)
            return Ok(cachedUser);
        User user = await _context.Users.Include(a => a.Todos).FirstOrDefaultAsync(a => a.Username == userDTO.Username && a.Password == userDTO.Password);
        if (user is null)
        {
            return NotFound();
        }
        bool isCached = _cacheService.Set<User>($"{user.Username}", user, this.expTime);
        if (!isCached)
            Log.Warning("I tried to cache user information but failed");
        return Ok(user);
    }
}
