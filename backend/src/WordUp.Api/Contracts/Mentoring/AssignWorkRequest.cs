namespace WordUp.Api.Contracts.Mentoring;

public record AssignWorkRequest(
    string ItemType,
    Guid ItemId,
    IReadOnlyList<Guid>? YouthIds,
    Guid? CohortId,
    DateTime? DueDate,
    string? ReminderCadence,
    string? Message);
