using Microsoft.AspNetCore.Mvc;
using TodoBackend.Data;

namespace TodoBackend.Controllers;

public class UsersController : ControllerBase
{
    private readonly TodosDbContext _context;
    public UsersController(TodosDbContext context)
    {
        _context = context;
    }
    [HttpPost("/register")]
    public async Task<IActionResult> Register()
    {
        return Ok();
    }
}
