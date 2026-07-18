namespace WordUp.Api.Contracts.ProgramAdministration;

public record EditCohortRequest(string Name, Guid? LeadMentorId, string? Location);
