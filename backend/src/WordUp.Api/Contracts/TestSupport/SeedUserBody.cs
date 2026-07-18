namespace WordUp.Api.Contracts.TestSupport;

/// <summary>
/// Request body for the Testing-only user seeding endpoint (<c>POST /api/_test/users</c>).
/// Never mapped outside the Testing environment.
/// </summary>
public record SeedUserBody(string Email, string Password, string DisplayName, string Role);
