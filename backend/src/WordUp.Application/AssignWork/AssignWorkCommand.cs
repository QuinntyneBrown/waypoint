using MediatR;

namespace WordUp.Application.AssignWork;

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
