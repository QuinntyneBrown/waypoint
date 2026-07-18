namespace WordUp.Api.Contracts.Authentication;

public record InviteRequest(string Email, string Role, Guid? CohortId, Guid? MentorId);
