using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Models;

public class Todo
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsFinished { get; set; } = false;
    public string Description { get; set; } = "";
    public int UserId { get; set; }
    public User User { get; set; }
}
