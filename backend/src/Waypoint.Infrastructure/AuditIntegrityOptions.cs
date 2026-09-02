using System.ComponentModel.DataAnnotations;

namespace Waypoint.Infrastructure;

public sealed class AuditIntegrityOptions
{
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;
}
