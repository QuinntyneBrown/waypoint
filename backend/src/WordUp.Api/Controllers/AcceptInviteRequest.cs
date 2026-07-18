namespace WordUp.Api.Controllers;

public record AcceptInviteRequest(string Token, string DisplayName, string Password, bool AgreeToGuidelines);
