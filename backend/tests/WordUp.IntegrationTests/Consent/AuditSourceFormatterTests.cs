using System.Net;
using Microsoft.AspNetCore.Http;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Consent;

[TestFixture]
[Property("Req", "L2-CONSENT-003")]
public sealed class AuditSourceFormatterTests
{
    [TestCase("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) Mobile Safari", null, "mobile")]
    [TestCase("Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) Mobile Safari", null, "tablet")]
    [TestCase("Mozilla/5.0 (Linux; Android 14; Pixel Tablet) AppleWebKit/537.36", null, "tablet")]
    [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64)", null, "desktop")]
    [TestCase("Mozilla/5.0 (compatible; Googlebot/2.1)", null, "automated")]
    [TestCase("private-custom-client/1.0", null, "unknown")]
    [TestCase("private-custom-client/1.0", "?1", "mobile")]
    public void Device_classification_is_coarse_and_canonical(
        string userAgent,
        string? mobileHint,
        string expected)
    {
        Assert.That(AuditSourceFormatter.ClassifyDevice(userAgent, mobileHint), Is.EqualTo(expected));
    }

    [TestCase("203.0.113.42", "203.0.113.0/24")]
    [TestCase("2001:db8:1234:5678:90ab:cdef:1234:5678", "2001:db8:1234::/48")]
    [TestCase("127.0.0.1", "loopback")]
    [TestCase("::1", "loopback")]
    public void Network_location_is_masked_to_a_non_host_prefix(string address, string expected)
    {
        Assert.That(
            AuditSourceFormatter.MinimizeNetworkLocation(IPAddress.Parse(address)),
            Is.EqualTo(expected));
    }

    [Test]
    public void Ipv4_mapped_ipv6_is_normalized_and_masked_as_ipv4()
    {
        var mapped = IPAddress.Parse("::ffff:203.0.113.42");

        Assert.That(
            AuditSourceFormatter.MinimizeNetworkLocation(mapped),
            Is.EqualTo("203.0.113.0/24"));
    }

    [Test]
    public void Missing_network_location_is_explicitly_unknown()
    {
        Assert.That(AuditSourceFormatter.MinimizeNetworkLocation(null), Is.EqualTo("unknown"));
    }

    [Test]
    public void Http_source_contains_required_coarse_context_without_raw_identifiers()
    {
        const string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) PRIVATE-UA-SENTINEL/17.3";
        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = userAgent;
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.42");

        var source = AuditSourceFormatter.Format(context);

        Assert.Multiple(() =>
        {
            Assert.That(source,
                Is.EqualTo("channel=api;device=desktop;location=203.0.113.0/24"));
            Assert.That(source, Does.Not.Contain(userAgent));
            Assert.That(source, Does.Not.Contain("PRIVATE-UA-SENTINEL"));
            Assert.That(source, Does.Not.Contain("203.0.113.42"));
        });
    }

    [Test]
    public void Application_source_has_explicit_service_and_server_context()
    {
        Assert.That(
            AuditSourceFormatter.Format(null),
            Is.EqualTo("channel=application;device=service;location=server"));
    }
}
