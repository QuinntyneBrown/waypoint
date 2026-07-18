namespace WordUp.Api.Controllers;

public record InviteRequest(string Email, string Role, Guid? CohortId, Guid? MentorId);
