using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public sealed record GetAssignmentQuery(Guid AssignmentId) : IRequest<AssignmentDetail>;

public sealed record AssignmentDetail(
    Guid Id,
    Guid? ModuleId,
    Guid? LessonId,
    string Title,
    string TaskDescription,
    IReadOnlyList<string> SubmissionTypes,
    DateTime? DueDate,
    int GracePeriodDays,
    bool AllowLate,
    bool RequireMentorApproval,
    int TotalPoints,
    string Status,
    IReadOnlyList<AssignmentRubricDetail> Rubric);

public sealed record AssignmentRubricDetail(
    Guid Id,
    string Description,
    int Points,
    int Order);
