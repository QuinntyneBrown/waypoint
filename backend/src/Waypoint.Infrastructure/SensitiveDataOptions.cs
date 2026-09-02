using System.ComponentModel.DataAnnotations;

namespace Waypoint.Infrastructure;

public sealed class SensitiveDataOptions
{
    [Required]
    [MinLength(32)]
    public string EncryptionKey { get; set; } = string.Empty;
}
