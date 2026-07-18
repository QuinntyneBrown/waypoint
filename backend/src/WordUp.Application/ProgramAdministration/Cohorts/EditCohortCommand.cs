using MediatR;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public record EditCohortCommand(Guid Id, string Name, Guid? LeadMentorId, string? Location) : IRequest;
