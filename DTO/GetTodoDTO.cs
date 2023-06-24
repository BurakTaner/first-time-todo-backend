namespace TodoBackend.DTO;

public class GetTodoDTO
{

    public GetTodoDTO(int id, string title, string description, bool isFinished)
    {
        this.Id = id;
        this.Title = title;
        this.Description = description;
        this.IsFinished = isFinished;
    }

    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsFinished { get; set; }
}
