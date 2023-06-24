
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoBackend.Data;
using TodoBackend.DTO;
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

    [HttpGet("/todos/{userId}")]
    public async Task<IActionResult> GetUserTodos(int userId)
    {
        List<Todo> userTodos = await _context.Todos.Where(a => a.UserId == userId).ToListAsync();
        List<GetTodoDTO> getTodoDtos = userTodos.Select(a => new GetTodoDTO(a.Id, a.Title, a.Description, a.IsFinished)).ToList();
        return Ok(getTodoDtos);
    }

    [HttpGet("/todos/todo/{todoId}")]
    public async Task<IActionResult> GetTodo(int todoId)
    {
        Todo todo = await _context.Todos.FirstOrDefaultAsync(a => a.Id == todoId);
        if (todo is null)
        {
            return NotFound();
        }
        GetTodoDTO getTodoDTO = new GetTodoDTO(
                todo.Id,
                todo.Title,
                todo.Description,
                todo.IsFinished
                );
        return Ok(getTodoDTO);
    }

    [HttpPost("/todos")]
    public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDTO createTodoDTO)
    {
        Todo todo = new Todo(
                createTodoDTO.Title,
                createTodoDTO.Description,
                createTodoDTO.UserId
                );
        try
        {
            await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Created("/todos", todo);
    }

    [HttpDelete("/todos/{todoId}")]
    public async Task<IActionResult> DeleteTodo(int todoId)
    {
        Todo todo = await _context.Todos.FirstOrDefaultAsync(a => a.Id == todoId);
        if (todo is null)
        {
            return NotFound();
        }
        try
        {
            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return NoContent();
    }

    [HttpPut("/todos/{todoId}")]
    public async Task<IActionResult> UpdateTodo([FromBody] UpdateTodoDTO updateTodoDTO, int todoId)
    {
        Todo todo = await _context.Todos.FirstOrDefaultAsync(a => a.Id == todoId);
        if (todo is null) return NotFound();
        try
        {
            todo.Update(
                    updateTodoDTO.Title,
                    updateTodoDTO.Description,
                    updateTodoDTO.IsFinished
                    );
            _context.Todos.Update(todo);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return NoContent();
    }
}
