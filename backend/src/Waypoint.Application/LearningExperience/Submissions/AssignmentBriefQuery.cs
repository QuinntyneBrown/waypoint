using MediatR;

namespace Waypoint.Application.LearningExperience.Submissions;

public sealed record AssignmentBriefQuery(Guid AssignmentId) : IRequest<AssignmentBriefView>;

public sealed record AssignmentRubricView(Guid Id, string Description, int Points);

public sealed record AssignmentBriefView(
    Guid Id,
    string Title,
    string TaskDescription,
    DateTime? DueDate,
    IReadOnlyList<string> AllowedSubmissionTypes,
    IReadOnlyList<AssignmentRubricView> Rubric,
    int TotalPoints,
    int MaxUploadBytes,
    Guid? ExistingSubmissionId,
    string? ExistingSubmissionStatus);
