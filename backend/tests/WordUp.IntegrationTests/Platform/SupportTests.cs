// Acceptance Test
// Traces to: L2-PLATFORM-005
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-005")]
public sealed class SupportTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-PLATFORM-005")]
    public async Task Submitting_a_support_request_persists_it_and_emails_the_director()
    {
        // Traces to: L2-PLATFORM-005
        const string director = "director@wordup.local";
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.OrgSettings.Add(new OrgSettings { Name = "Word Up", ContactEmail = director });
            await db.SaveChangesAsync();
        }

        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        var response = await SendAsync(HttpMethod.Post, "/api/support", body: new
        {
            category = "Bug",
            description = "The lesson video will not play on my phone.",
            replyToEmail = "youth.reply@example.com",
        }, token: token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<SupportRequestResultDto>();
        Assert.That(result!.Id, Is.Not.EqualTo(Guid.Empty));

        // The request is persisted.
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = db.SupportRequests.Single(s => s.Id == result.Id);
            Assert.That(stored.Category, Is.EqualTo("Bug"));
            Assert.That(stored.ReplyToEmail, Is.EqualTo("youth.reply@example.com"));
        }

        // An email was routed to the configured director inbox.
        var sent = Factory.Emails.LastTo(director);
        Assert.That(sent, Is.Not.Null, "a support email should be sent to the director inbox");
    }

    private sealed record SupportRequestResultDto(Guid Id);
}
