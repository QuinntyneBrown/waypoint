using System.ComponentModel.DataAnnotations;

namespace WordUp.Infrastructure;

public class JwtOptions
{
    [Required]
    public string Issuer { get; set; } = "WordUp";

    [Required]
    public string Audience { get; set; } = "WordUp";

    // Fail fast if the signing key is missing or too weak for HS256 (L2-NFR-011 AC4).
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;
}
