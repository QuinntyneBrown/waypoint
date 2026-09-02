using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed record PublishAssignmentCommand(
    Guid AssignmentId,
    PublishStatus Status) : IRequest;
