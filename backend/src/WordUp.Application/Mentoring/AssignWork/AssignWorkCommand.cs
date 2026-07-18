using MediatR;

namespace WordUp.Application.Mentoring.AssignWork;

/// <summary>A mentor assigns a lesson/module/project to specific youth or a whole cohort (L2-MENTOR-008).</summary>
public record AssignWorkCommand(
    string ItemType,
    Guid ItemId,
    IReadOnlyList<Guid>? YouthIds,
    Guid? CohortId,
    DateTime? DueDate,
    string? ReminderCadence,
    string? Message) : IRequest<AssignWorkResult>;

public record AssignWorkResult(int Created);

public static class AssignWorkItemTypes
{
    public const string Lesson = "Lesson";
    public const string Module = "Module";
    public const string Project = "Project";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Lesson, Module, Project };
}

public static class ReminderCadences
{
    public const string OneDayBefore = "1-day-before";
    public const string TwoDaysBefore = "2-days-before";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { OneDayBefore, TwoDaysBefore };

    public static TimeSpan Offset(string cadence) => cadence.ToLowerInvariant() switch
    {
        OneDayBefore => TimeSpan.FromDays(1),
        TwoDaysBefore => TimeSpan.FromDays(2),
        _ => TimeSpan.Zero,
    };
}
