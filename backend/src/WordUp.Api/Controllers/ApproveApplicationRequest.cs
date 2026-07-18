namespace WordUp.Api.Controllers;

public record ApproveApplicationRequest(Guid CohortId, Guid MentorId, string? TrackInterest, string? DecisionNote);
