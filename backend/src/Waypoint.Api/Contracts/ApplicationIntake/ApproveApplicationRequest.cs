namespace Waypoint.Api.Contracts.ApplicationIntake;

public record ApproveApplicationRequest(Guid CohortId, Guid MentorId, string? TrackInterest, string? DecisionNote);
