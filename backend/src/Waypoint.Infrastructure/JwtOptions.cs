using System.ComponentModel.DataAnnotations;

namespace Waypoint.Infrastructure;

public class JwtOptions
{
    [Required]
    public string Issuer { get; set; } = "Waypoint";

    [Required]
    public string Audience { get; set; } = "Waypoint";

    // Fail fast if the signing key is missing or too weak for HS256 (L2-NFR-011 AC4).
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;
}
