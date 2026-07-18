namespace WordUp.Api.Contracts.PlatformServices;

public record SubmitSupportRequest(string Category, string Description, string ReplyToEmail, string? ScreenshotUrl);
