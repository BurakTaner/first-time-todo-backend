using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Models;

public class Todo
{
    public Todo(string title, string description, int userId)
    {
        this.Title = title;
        this.Description = description;
        this.UserId = userId;
    }

    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsFinished { get; set; } = false;
    public string Description { get; set; } = "";
    public int UserId { get; set; }
    public User User { get; set; }


    public void Update(string title, string description, bool isFinished)
    {
        this.Title = title;
        this.Description = description;
        this.IsFinished = isFinished;
    }


    public Todo()
    {

    }
}
