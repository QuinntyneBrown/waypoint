using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ConsentAndSafeguarding;

public class AuditLogQueryHandler : IRequestHandler<AuditLogQuery, AuditLogResult>
{
    private const string Redacted = "[redacted]";
    private readonly IAppDbContext _db;
    private readonly IAuditIntegrity _integrity;

    public AuditLogQueryHandler(IAppDbContext db, IAuditIntegrity integrity)
    {
        _db = db;
        _integrity = integrity;
    }

    public async Task<AuditLogResult> Handle(AuditLogQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;
        var end = request.To is null
            ? null
            : request.To.Value.TimeOfDay == TimeSpan.Zero
                ? request.To.Value.Date.AddDays(1)
                : request.To;

        var allEntries = _db.AuditEntries.AsNoTracking();
        var integrityEntries = await allEntries
            .OrderBy(entry => entry.SequenceNumber)
            .ToListAsync(cancellationToken);
        var integrityValid = _integrity.VerifyChain(integrityEntries);
        var actions = await allEntries
            .Select(entry => entry.Action)
            .Distinct()
            .OrderBy(action => action)
            .ToListAsync(cancellationToken);
        var actorIds = await allEntries
            .Where(entry => entry.ActorUserId != null)
            .Select(entry => entry.ActorUserId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
        var actorNames = await _db.Users
            .AsNoTracking()
            .Where(user => actorIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
        var actors = actorIds
            .Select(id => new AuditActorOption(
                id,
                actorNames.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name)
                    ? name
                    : $"Unknown user · {id.ToString()[..8]}"))
            .OrderBy(actor => actor.Name)
            .ThenBy(actor => actor.Id)
            .ToList();

        var query = allEntries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(e => e.Action == request.Action);
        }

        if (request.ActorUserId is not null)
        {
            query = query.Where(e => e.ActorUserId == request.ActorUserId);
        }

        if (request.From is DateTime from)
        {
            query = query.Where(e => e.At >= from);
        }

        if (end is DateTime endExclusive)
        {
            query = query.Where(e => e.At < endExclusive);
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
                e.ActorUserId is Guid actorId
                    ? actorNames.GetValueOrDefault(actorId, $"Unknown user · {actorId.ToString()[..8]}")
                    : "System",
                e.Action,
                e.TargetType,
                e.TargetId,
                e.At,
                e.Source,
                e.IsPrivate,
                e.IsPrivate ? Redacted : e.Details))
            .ToList();

        return new AuditLogResult(items, page, pageSize, total, actors, actions, integrityValid);
    }
}
