using System.ComponentModel.DataAnnotations;

namespace TodoBackend.DTO;

public class TokenRequestDTO
{
    [Required]
    public string Jwt { get; set; } = "";
    [Required]
    public string RefreshToken { get; set; } = "";
}
