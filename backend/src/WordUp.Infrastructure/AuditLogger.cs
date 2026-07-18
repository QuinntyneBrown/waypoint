using Microsoft.AspNetCore.Http;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Infrastructure;

public sealed class AuditLogger : IAuditLogger
{
    private readonly IAppDbContext _db;
    private readonly IHttpContextAccessor _http;
    private readonly IClock _clock;

    public AuditLogger(IAppDbContext db, IHttpContextAccessor http, IClock clock)
    {
        _db = db;
        _http = http;
        _clock = clock;
    }

    public async Task LogAsync(
        string action,
        Guid? actorUserId = null,
        string? targetType = null,
        Guid? targetId = null,
        bool isPrivate = false,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        var ctx = _http.HttpContext;
        var ip = ctx?.Connection.RemoteIpAddress?.ToString();
        var userAgent = ctx?.Request.Headers.UserAgent.ToString();
        var source = string.Join(" | ", new[] { ip, userAgent }.Where(s => !string.IsNullOrWhiteSpace(s)));

        _db.AuditEntries.Add(new AuditEntry
        {
            Action = action,
            ActorUserId = actorUserId,
            TargetType = targetType,
            TargetId = targetId,
            At = _clock.UtcNow,
            Source = string.IsNullOrWhiteSpace(source) ? null : source,
            IsPrivate = isPrivate,
            Details = details,
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
