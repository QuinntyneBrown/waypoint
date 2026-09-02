namespace Waypoint.Api.Contracts.ProgramAdministration;

public sealed record SetCohortMentorsRequest(
    Guid LeadMentorId,
    IReadOnlyList<Guid> SupportingMentorIds);
