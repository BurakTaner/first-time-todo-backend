namespace TodoBackend.DTO;

public class CreateTodoDTO
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int UserId { get; set; }
}
