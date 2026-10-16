using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.NonFunctional;

// Acceptance Tests
// Traces to: L2-AUTH-012 AC4; L2-NFR-002 AC2-AC4
[TestFixture]
[Property("Req", "L2-AUTH-012")]
[Property("Req", "L2-NFR-002")]
public sealed class TransportSecurityTests
{
    [Test]
    public async Task Production_rejects_plain_http_while_serving_https_with_security_headers()
    {
        await using var factory = new SecurityConfiguredFactory(
            "Production",
            new[] { "https://portal.waypoint.example" });
        using var http = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            AllowAutoRedirect = false,
        });
        using var https = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        var rejected = await http.GetAsync("/health");
        var accepted = await https.GetAsync("/health");

        Assert.Multiple(() =>
        {
            Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(rejected.Headers.Contains("Strict-Transport-Security"), Is.True);
            Assert.That(accepted.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [TestCase(
        "Production",
        "https://portal.waypoint.example",
        "http://localhost:4200")]
    [TestCase(
        "Development",
        "http://localhost:4300",
        "https://portal.waypoint.example")]
    public async Task Cors_allows_only_the_origins_configured_for_the_active_environment(
        string environment,
        string approvedOrigin,
        string rejectedOrigin)
    {
        await using var factory = new SecurityConfiguredFactory(environment, new[] { approvedOrigin });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(environment == "Production" ? "https://localhost" : "http://localhost"),
            AllowAutoRedirect = false,
        });

        var approved = await PreflightAsync(client, approvedOrigin);
        var rejected = await PreflightAsync(client, rejectedOrigin);

        Assert.Multiple(() =>
        {
            Assert.That(approved.Headers.TryGetValues("Access-Control-Allow-Origin", out var values), Is.True);
            Assert.That(values, Is.EquivalentTo(new[] { approvedOrigin }));
            Assert.That(rejected.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
        });
    }

    [TestCase("Production", HttpStatusCode.NotFound)]
    [TestCase("Development", HttpStatusCode.OK)]
    public async Task Swagger_is_served_only_outside_production(string environment, HttpStatusCode expected)
    {
        await using var factory = new SecurityConfiguredFactory(
            environment,
            new[] { environment == "Production" ? "https://portal.waypoint.example" : "http://localhost:4300" });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(environment == "Production" ? "https://localhost" : "http://localhost"),
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.That(response.StatusCode, Is.EqualTo(expected));
    }

    [Test]
    public void Production_refuses_a_configured_plain_http_cors_origin()
    {
        using var factory = new SecurityConfiguredFactory(
            "Production",
            new[] { "http://portal.waypoint.example" });

        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
            }));

        Assert.That(error!.Message, Does.Contain("must use HTTPS"));
    }

    [TestCase("/api/auth/sign-in")]
    [TestCase("/api/auth/password-reset/request")]
    [TestCase("/api/invitations")]
    [TestCase("/api/people/00000000-0000-0000-0000-000000000001/send-password-reset")]
    public async Task Every_sensitive_auth_reset_and_invitation_route_is_rate_limited(string path)
    {
        await using var factory = new SecurityConfiguredFactory(
            "Development",
            new[] { "http://localhost:4300" },
            permitLimit: 2);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            AllowAutoRedirect = false,
        });

        var first = await client.PostAsJsonAsync(path, BodyFor(path));
        var second = await client.PostAsJsonAsync(path, BodyFor(path));
        var limited = await client.PostAsJsonAsync(path, BodyFor(path));

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.Not.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(second.StatusCode, Is.Not.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(limited.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(limited.Headers.Contains("Retry-After"), Is.True);
        });
    }

    [Test]
    public async Task Non_sensitive_routes_do_not_consume_the_sensitive_request_window()
    {
        await using var factory = new SecurityConfiguredFactory(
            "Development",
            new[] { "http://localhost:4300" },
            permitLimit: 1);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            AllowAutoRedirect = false,
        });

        var responses = await Task.WhenAll(
            client.GetAsync("/health"),
            client.GetAsync("/health"),
            client.GetAsync("/health"));

        Assert.That(responses.Select(response => response.StatusCode),
            Is.All.EqualTo(HttpStatusCode.OK));
    }

    private static object BodyFor(string path) => path switch
    {
        "/api/auth/sign-in" => new { email = "missing@example.com", password = "WrongPass1!" },
        "/api/auth/password-reset/request" => new { email = "missing@example.com" },
        _ => new { },
    };

    private static Task<HttpResponseMessage> PreflightAsync(HttpClient client, string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/sign-in");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        return client.SendAsync(request);
    }

    private sealed class SecurityConfiguredFactory : WebApplicationFactory<Program>
    {
        private readonly string _environment;
        private readonly IReadOnlyList<string> _allowedOrigins;
        private readonly int _permitLimit;

        public SecurityConfiguredFactory(
            string environment,
            IReadOnlyList<string> allowedOrigins,
            int permitLimit = 20)
        {
            _environment = environment;
            _allowedOrigins = allowedOrigins;
            _permitLimit = permitLimit;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(_environment);
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                var values = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = _environment == "Production"
                        ? "Server=production;Database=Waypoint;User Id=waypoint;Password=not-used-by-test;TrustServerCertificate=True"
                        : "InMemory",
                    ["Jwt:SigningKey"] = "waypoint-transport-tests-signing-key-32chars-min!!",
                    ["AuditIntegrity:SigningKey"] = "waypoint-transport-tests-audit-key-32chars-minimum!!",
                    ["SensitiveData:EncryptionKey"] = "waypoint-transport-tests-encryption-key-32chars!!",
                    ["ApplicationUrls:PortalBaseUrl"] = "https://portal.waypoint.example",
                    ["ApplicationUrls:PublicSiteBaseUrl"] = "https://waypoint.example",
                    ["Smtp:Enabled"] = (_environment == "Production").ToString(),
                    ["Smtp:Host"] = "smtp.waypoint.example",
                    ["Smtp:Port"] = "587",
                    ["Smtp:UseSsl"] = "false",
                    ["Smtp:UseStartTls"] = "true",
                    ["Smtp:FromEmail"] = "noreply@waypoint.example",
                    ["Security:SensitiveRateLimit:PermitLimit"] = _permitLimit.ToString(),
                    ["Security:SensitiveRateLimit:WindowSeconds"] = "60",
                };
                for (var index = 0; index < _allowedOrigins.Count; index++)
                {
                    values[$"Cors:AllowedOrigins:{index}"] = _allowedOrigins[index];
                }
                configuration.AddInMemoryCollection(values);
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDatabaseLifecycle>();
                services.AddSingleton<IDatabaseLifecycle, NoOpDatabaseLifecycle>();
            });
        }
    }

    private sealed class NoOpDatabaseLifecycle : IDatabaseLifecycle
    {
        public Task MigrateAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ResetAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
