using MediatR;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public sealed record YouthHomeQuery : IRequest<YouthHomeView>;

public sealed record YouthHomeView(
    bool Placed,
    string? CohortName,
    string? MentorName,
    int CompletedItems,
    int TotalItems,
    double OverallPercent,
    HomeLearningItem? ContinueLearning,
    HomeSession? NextSession,
    IReadOnlyList<HomeAssignedWork> AssignedWork);

public sealed record HomeLearningItem(
    Guid ItemId,
    string Type,
    string Title,
    string ModuleName,
    string Status,
    int? EstimatedMinutes,
    string TargetPath);

public sealed record HomeSession(
    Guid SessionId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    string? RsvpResponse);

public sealed record HomeAssignedWork(
    Guid AssignmentId,
    Guid ItemId,
    string ItemType,
    string Title,
    DateTime? DueDate,
    string Status,
    bool Overdue,
    string? Message,
    string TargetPath);
