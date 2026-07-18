namespace WordUp.Api.Contracts.ProgramAdministration;

public record CreateCohortRequest(
    string Name,
    Guid TrackId,
    Guid LeadMentorId,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Location);
