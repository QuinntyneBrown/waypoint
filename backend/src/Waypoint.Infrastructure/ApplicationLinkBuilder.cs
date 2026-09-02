using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public sealed class ApplicationLinkBuilder : IApplicationLinkBuilder
{
    private readonly Uri _portal;
    private readonly Uri _publicSite;

    public ApplicationLinkBuilder(IOptions<ApplicationUrlOptions> options)
    {
        _portal = BaseUri(options.Value.PortalBaseUrl);
        _publicSite = BaseUri(options.Value.PublicSiteBaseUrl);
    }

    public string Portal(string relativePath) => Build(_portal, relativePath);
    public string PublicSite(string relativePath) => Build(_publicSite, relativePath);

    private static Uri BaseUri(string value) => new(value.TrimEnd('/') + "/", UriKind.Absolute);

    private static string Build(Uri root, string relativePath) =>
        new Uri(root, relativePath.TrimStart('/')).AbsoluteUri;
}
