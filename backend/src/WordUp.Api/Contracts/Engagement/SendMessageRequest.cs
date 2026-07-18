namespace WordUp.Api.Contracts.Engagement;

public record SendMessageRequest(Guid ToUserId, string Body);
