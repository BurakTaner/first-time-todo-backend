namespace TodoBackend.DTO;

public class UpdateTodoDTO
{
    public UpdateTodoDTO(string title, string description, bool isFinished)
    {
        this.Title = title;
        this.Description = description;
        this.IsFinished = isFinished;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsFinished { get; set; }

    public UpdateTodoDTO()
    {

    }
}
