using System.ComponentModel.DataAnnotations;

namespace Waypoint.Infrastructure;

public sealed class ApplicationUrlOptions
{
    [Required, Url]
    public string PortalBaseUrl { get; set; } = string.Empty;

    [Required, Url]
    public string PublicSiteBaseUrl { get; set; } = string.Empty;
}
