namespace WordUp.Api.Controllers;

public record RegisterRequest(string Email, string Password, string DisplayName);
