namespace WordUp.Infrastructure;

public class JwtOptions
{
    public string Issuer { get; set; } = "WordUp";
    public string Audience { get; set; } = "WordUp";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
}
