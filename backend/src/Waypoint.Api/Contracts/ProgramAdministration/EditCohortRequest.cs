namespace Waypoint.Api.Contracts.ProgramAdministration;

public record EditCohortRequest(string Name, Guid? LeadMentorId, string? Location);
