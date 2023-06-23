
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoBackend.Data;
using TodoBackend.Models;

namespace TodoBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodosDbContext _context;
    public TodosController(TodosDbContext context)
    {
        _context = context;
    }

    [HttpGet("/todos/{id}")]
    public async Task<IActionResult> GetUserTodos(int id)
    {
        List<Todo> userTodos = await _context.Todos.Where(a => a.UserId == id).ToListAsync();
        return Ok(userTodos);
    }
}
