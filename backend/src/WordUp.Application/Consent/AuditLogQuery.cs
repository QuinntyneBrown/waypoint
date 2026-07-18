using MediatR;

namespace WordUp.Application.Consent;

public record AuditLogQuery(int Page = 1, int PageSize = 50, string? Action = null, Guid? ActorUserId = null)
    : IRequest<AuditLogResult>;
