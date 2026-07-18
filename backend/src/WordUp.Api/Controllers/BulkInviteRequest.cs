namespace WordUp.Api.Controllers;

public record BulkInviteRequest(string[] Emails, string Role);
