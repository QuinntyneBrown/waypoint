namespace Waypoint.Application.ConsentAndSafeguarding;

public record AuditLogResult(
    IReadOnlyList<AuditLogItem> Items,
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<AuditActorOption> Actors,
    IReadOnlyList<string> Actions,
    bool IntegrityValid);

public record AuditActorOption(Guid Id, string Name);

public record AuditLogItem(
    Guid Id,
    Guid? ActorUserId,
    string ActorName,
    string Action,
    string? TargetType,
    Guid? TargetId,
    DateTime At,
    string? Source,
    bool IsPrivate,
    string? Details);
