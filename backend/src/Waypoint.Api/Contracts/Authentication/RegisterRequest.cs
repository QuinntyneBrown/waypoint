namespace Waypoint.Api.Contracts.Authentication;

public record RegisterRequest(string Email, string Password, string DisplayName);
