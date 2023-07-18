namespace TodoBackend.Configuration;

public class JwtConfig
{
    public string SecretKey { get; set; } = string.Empty;
    public TimeSpan ExpiryTime { get; set; }
}
