using MediatR;

namespace WordUp.Application.ProgramAdministration.MentorAssignments;

public sealed record SetCohortMentorsCommand(
    Guid CohortId,
    Guid LeadMentorId,
    IReadOnlyList<Guid> SupportingMentorIds) : IRequest;
