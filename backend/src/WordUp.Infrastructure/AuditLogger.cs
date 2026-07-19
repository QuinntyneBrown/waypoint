using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

public sealed class AuditLogger : IAuditLogger
{
    private static readonly SemaphoreSlim WriteLock = new(1, 1);
    private readonly IAppDbContext _db;
    private readonly IHttpContextAccessor _http;
    private readonly IClock _clock;
    private readonly IAuditIntegrity _integrity;

    public AuditLogger(
        IAppDbContext db,
        IHttpContextAccessor http,
        IClock clock,
        IAuditIntegrity integrity)
    {
        _db = db;
        _http = http;
        _clock = clock;
        _integrity = integrity;
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
        var source = $"ip={ip ?? "unknown"} | user-agent={(string.IsNullOrWhiteSpace(userAgent) ? "unknown" : userAgent)}";

        await WriteLock.WaitAsync(cancellationToken);
        try
        {
            var previous = await _db.AuditEntries
                .AsNoTracking()
                .Where(entry => entry.SequenceNumber != null)
                .OrderByDescending(entry => entry.SequenceNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var entry = new AuditEntry
            {
                SequenceNumber = (previous?.SequenceNumber ?? 0) + 1,
                PreviousHash = previous?.IntegrityHash,
                Action = action,
                ActorUserId = actorUserId,
                TargetType = targetType,
                TargetId = targetId,
                At = _clock.UtcNow,
                Source = source,
                IsPrivate = isPrivate,
                Details = details,
            };
            entry.IntegrityHash = _integrity.ComputeHash(entry);
            _db.AuditEntries.Add(entry);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            WriteLock.Release();
        }
    }
}
