using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoBackend.Data;
using TodoBackend.DTO;
using TodoBackend.Models;
using System.Security.Cryptography;

namespace TodoBackend.Controllers;

[ApiController]
public class UsersController : ControllerBase
{
    private readonly TodosDbContext _context;
    public UsersController(TodosDbContext context)
    {
        _context = context;
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
            return Ok(user);
        }
        return BadRequest();
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
    {
        User user = await _context.Users.Include(a => a.Todos).FirstOrDefaultAsync(a => a.Username == userDTO.Username && a.Password == userDTO.Password);
        if (user is null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}
