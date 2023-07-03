
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
public class TodosController : ControllerBase
{
    private readonly TodosDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly DateTimeOffset expTime = DateTimeOffset.Now.AddMinutes(3);
    public TodosController(TodosDbContext context, ICacheService cacheService)

    {
        _context = context;
        _cacheService = cacheService;
    }

    [HttpGet("/todos/{userId}")]
    public async Task<IActionResult> GetUserTodos(int userId)
    {
        IEnumerable<GetTodoDTO> result = _cacheService.Get<IEnumerable<GetTodoDTO>>($"user{userId}-todos");
        if (result is not null)
            return Ok(result);

        IEnumerable<Todo> userTodos = await _context.Todos.Where(a => a.UserId == userId).ToListAsync();
        IEnumerable<GetTodoDTO> getTodoDtos = userTodos.Select(a => new GetTodoDTO(a.Id, a.Title, a.Description, a.IsFinished)).ToList();
        bool isCached = _cacheService.Set<IEnumerable<GetTodoDTO>>($"user{userId}-todos", getTodoDtos, this.expTime);
        if (!isCached)
            Log.Warning("I tried to cache all todos owned by a user but failed");
        return Ok(getTodoDtos);
    }

    [HttpGet("/todos/todo/{todoId}")]
    public async Task<IActionResult> GetTodo(int todoId)
    {
        GetTodoDTO result = _cacheService.Get<GetTodoDTO>($"todo{todoId}");
        if (result is not null)
            return Ok(result);

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
        bool isCached = _cacheService.Set<GetTodoDTO>($"todo{todoId}", getTodoDTO, this.expTime);
        if (!isCached)
            Log.Warning("I tried to cache a single todo but failed");
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
            EntityEntry<Todo> result = await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();
            bool isDeleted = _cacheService.Delete($"user{createTodoDTO.UserId}-todos");
            if (!isDeleted)
                Log.Warning($"I tried to delete cached todos of user {createTodoDTO.UserId} but failed");
            return Created("/todos", result.Entity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
            bool isDeleted = _cacheService.Delete($"todo{todoId}");
            if (!isDeleted)
                Log.Warning("I tried to delete a cached todo but failed");
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
            bool isDeletedAll = _cacheService.Delete($"user{todo.UserId}-todos");
            bool isDeletedSingle = _cacheService.Delete($"todo{todoId}");
            if (!isDeletedAll && !isDeletedSingle)
                Log.Warning("I tried to delete all todos and updated todo from cache but failed");
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
