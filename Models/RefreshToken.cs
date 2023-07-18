using System.ComponentModel.DataAnnotations;

namespace TodoBackend.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string JwtId { get; set; } = "";
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime ExpiryTime { get; set; }
}
