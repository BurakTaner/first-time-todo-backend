namespace TodoBackend.DTO;

public class AuthResponse
{
    public string Token { get; set; } = "";
    public bool Result { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
