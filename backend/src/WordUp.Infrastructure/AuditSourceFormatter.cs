using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;

namespace WordUp.Infrastructure;

/// <summary>
/// Produces the device-and-location source required for safeguarding audit evidence without
/// retaining a raw user-agent or a host-level network address.
/// </summary>
public static class AuditSourceFormatter
{
    private const int MaximumUserAgentCharactersInspected = 512;

    public static string Format(HttpContext? context)
    {
        if (context is null)
        {
            return "channel=application;device=service;location=server";
        }

        var device = ClassifyDevice(
            context.Request.Headers.UserAgent.ToString(),
            context.Request.Headers["Sec-CH-UA-Mobile"].ToString());
        var location = MinimizeNetworkLocation(context.Connection.RemoteIpAddress);
        return $"channel=api;device={device};location={location}";
    }

    public static string ClassifyDevice(string? userAgent, string? mobileHint = null)
    {
        var inspected = userAgent ?? string.Empty;
        if (inspected.Length > MaximumUserAgentCharactersInspected)
        {
            inspected = inspected[..MaximumUserAgentCharactersInspected];
        }

        if (ContainsAny(inspected, "bot", "crawler", "spider", "headless", "playwright"))
        {
            return "automated";
        }

        if (ContainsAny(inspected, "ipad", "tablet", "kindle", "silk/"))
        {
            return "tablet";
        }

        if (string.Equals(mobileHint?.Trim(), "?1", StringComparison.Ordinal)
            || ContainsAny(inspected, "iphone", "ipod", "windows phone", "android")
            && inspected.Contains("mobile", StringComparison.OrdinalIgnoreCase))
        {
            return "mobile";
        }

        if (inspected.Contains("android", StringComparison.OrdinalIgnoreCase))
        {
            return "tablet";
        }

        if (ContainsAny(inspected, "windows nt", "macintosh", "x11", "cros", "linux x86_64"))
        {
            return "desktop";
        }

        return "unknown";
    }

    public static string MinimizeNetworkLocation(IPAddress? address)
    {
        if (address is null)
        {
            return "unknown";
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address))
        {
            return "loopback";
        }

        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            bytes[3] = 0;
            return $"{new IPAddress(bytes)}/24";
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            Array.Clear(bytes, 6, bytes.Length - 6);
            return $"{new IPAddress(bytes)}/48";
        }

        return "unknown";
    }

    private static bool ContainsAny(string value, params string[] markers) =>
        markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
}
