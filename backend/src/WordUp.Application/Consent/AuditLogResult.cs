namespace WordUp.Application.Consent;

public record AuditLogResult(IReadOnlyList<AuditLogItem> Items, int Page, int PageSize, int Total);

public record AuditLogItem(
    Guid Id,
    Guid? ActorUserId,
    string Action,
    string? TargetType,
    Guid? TargetId,
    DateTime At,
    string? Source,
    bool IsPrivate,
    string? Details);
