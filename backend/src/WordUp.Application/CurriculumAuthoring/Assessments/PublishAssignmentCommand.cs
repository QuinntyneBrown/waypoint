using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public sealed record PublishAssignmentCommand(
    Guid AssignmentId,
    PublishStatus Status) : IRequest;
