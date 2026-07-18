namespace WordUp.Api.Contracts.PublicSite;

public record ContactRequestBody(string Name, string Email, string Topic, string Message);
