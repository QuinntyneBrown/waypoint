namespace WordUp.Api.Contracts.Authentication;

public record BulkInviteRequest(string[] Emails, string Role, Guid? CohortId, Guid? MentorId);
