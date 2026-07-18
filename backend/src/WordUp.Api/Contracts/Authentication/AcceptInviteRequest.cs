namespace WordUp.Api.Contracts.Authentication;

public record AcceptInviteRequest(string Token, string DisplayName, string Password, bool AgreeToGuidelines);
