// Acceptance Test
// Traces to: L2-NFR-008 (performance targets: list views use server-side paging so payloads stay
//            bounded regardless of org size)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Category("Performance")]
[Property("Req", "L2-NFR-008")]
public sealed class PagingTests : IntegrationTestBase
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

    private async Task SeedUserAsync(string role, string password = "Passw0rd!")
    {
        var email = $"page.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User
        {
            Email = email,
            DisplayName = $"Page {role}",
            Role = role,
            Status = UserStatus.Active,
            PasswordHash = hasher.Hash(password),
        });
        await db.SaveChangesAsync();
    }

    [Test]
    [Property("Req", "L2-NFR-008")]
    public async Task People_list_is_server_side_paged_so_the_payload_is_bounded_by_page_size()
    {
        // Traces to: L2-NFR-008 AC2
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        // Seed more people than fit on a single small page so the total exceeds one page.
        const int pageSize = 3;
        for (var i = 0; i < pageSize + 4; i++)
        {
            await SeedUserAsync(Roles.Youth);
        }

        var response = await SendAsync(HttpMethod.Get, $"/api/people?page=1&pageSize={pageSize}", token: adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var page = await response.Content.ReadFromJsonAsync<PagedPeopleDto>();
        Assert.That(page!.Items.Count, Is.EqualTo(pageSize), "the page payload is bounded by pageSize");
        Assert.That(page.Page, Is.EqualTo(1));
        Assert.That(page.PageSize, Is.EqualTo(pageSize));
        Assert.That(page.Total, Is.GreaterThan(pageSize), "Total reflects the full unpaged count, so more pages exist");
    }

    private sealed record PagedPeopleDto(List<PersonSummaryDto> Items, int Total, int Page, int PageSize);

    private sealed record PersonSummaryDto(
        Guid Id, string DisplayName, string Email, string Role, string Status,
        Guid? CohortId, Guid? MentorId, DateTime? LastActive);
}
