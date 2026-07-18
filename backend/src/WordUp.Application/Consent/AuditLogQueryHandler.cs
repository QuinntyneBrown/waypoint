using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;

namespace WordUp.Application.Consent;

public class AuditLogQueryHandler : IRequestHandler<AuditLogQuery, AuditLogResult>
{
    private const string Redacted = "[redacted]";
    private readonly IAppDbContext _db;

    public AuditLogQueryHandler(IAppDbContext db) => _db = db;

    public async Task<AuditLogResult> Handle(AuditLogQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var query = _db.AuditEntries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(e => e.Action == request.Action);
        }

        if (request.ActorUserId is not null)
        {
            query = query.Where(e => e.ActorUserId == request.ActorUserId);
        }

        var total = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(e => e.At)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Private entries never expose their detail in the log view (L2-CONSENT-003 AC2).
        var items = entries
            .Select(e => new AuditLogItem(
                e.Id,
                e.ActorUserId,
                e.Action,
                e.TargetType,
                e.TargetId,
                e.At,
                e.Source,
                e.IsPrivate,
                e.IsPrivate ? Redacted : e.Details))
            .ToList();

        return new AuditLogResult(items, page, pageSize, total);
    }
}
