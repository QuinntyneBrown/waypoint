using System.Globalization;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Engagement.Attendance;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ReportingAndData.DataTransfer;

public record ExportAuditLogQuery() : IRequest<CsvFile>;

public sealed record ExportAuditLogFileQuery(
    string Format,
    string? Action = null,
    Guid? ActorUserId = null,
    DateTime? From = null,
    DateTime? To = null) : IRequest<DataExportFile>;

public class ExportAuditLogQueryHandler : IRequestHandler<ExportAuditLogQuery, CsvFile>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public ExportAuditLogQueryHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<CsvFile> Handle(ExportAuditLogQuery request, CancellationToken cancellationToken)
    {
        // Note: private note content is never persisted in AuditEntry.Details, and Details is not exported here.
        var entries = await _db.AuditEntries
            .OrderByDescending(a => a.At)
            .Select(a => new
            {
                a.Id,
                a.ActorUserId,
                a.Action,
                a.TargetType,
                a.TargetId,
                a.At,
            })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.Append(Csv.ToRow(new[] { "Id", "ActorUserId", "Action", "TargetType", "TargetId", "At" }));
        sb.Append("\r\n");

        foreach (var e in entries)
        {
            sb.Append(Csv.ToRow(new[]
            {
                e.Id.ToString(),
                e.ActorUserId?.ToString() ?? string.Empty,
                e.Action,
                e.TargetType ?? string.Empty,
                e.TargetId?.ToString() ?? string.Empty,
                e.At.ToString("o", CultureInfo.InvariantCulture),
            }));
            sb.Append("\r\n");
        }

        await _audit.LogAsync("DataExported", _currentUser.UserId, "AuditLog",
            cancellationToken: cancellationToken);

        return new CsvFile("audit-log.csv", sb.ToString());
    }
}

public sealed class ExportAuditLogFileQueryHandler
    : IRequestHandler<ExportAuditLogFileQuery, DataExportFile>
{
    private const string Redacted = "[redacted]";
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;

    public ExportAuditLogFileQueryHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IAuditLogger audit,
        IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
        _clock = clock;
    }

    public async Task<DataExportFile> Handle(
        ExportAuditLogFileQuery request,
        CancellationToken cancellationToken)
    {
        var format = request.Format.Trim().ToLowerInvariant();
        if (format is not ("csv" or "pdf"))
            throw new ValidationException("Export format must be csv or pdf.");

        var end = request.To is null
            ? null
            : request.To.Value.TimeOfDay == TimeSpan.Zero
                ? request.To.Value.Date.AddDays(1)
                : request.To;
        var query = _db.AuditEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(item => item.Action == request.Action);
        if (request.ActorUserId is Guid actorUserId)
            query = query.Where(item => item.ActorUserId == actorUserId);
        if (request.From is DateTime from)
            query = query.Where(item => item.At >= from);
        if (end is DateTime endExclusive)
            query = query.Where(item => item.At < endExclusive);

        var entries = await query
            .OrderByDescending(item => item.At)
            .ThenByDescending(item => item.Id)
            .ToListAsync(cancellationToken);
        var rows = new List<string>
        {
            Csv.ToRow(["Id", "ActorUserId", "Action", "TargetType", "TargetId", "At", "Source", "Private", "Details"]),
        };
        rows.AddRange(entries.Select(item => Csv.ToRow([
            item.Id.ToString(),
            item.ActorUserId?.ToString() ?? string.Empty,
            item.Action,
            item.TargetType ?? string.Empty,
            item.TargetId?.ToString() ?? string.Empty,
            item.At.ToString("o", CultureInfo.InvariantCulture),
            item.Source ?? string.Empty,
            item.IsPrivate ? "Yes" : "No",
            item.IsPrivate ? Redacted : item.Details ?? string.Empty,
        ])));

        var stamp = _clock.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var file = format == "csv"
            ? new DataExportFile(
                $"audit-log-{stamp}.csv",
                "text/csv; charset=utf-8",
                Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, rows) + Environment.NewLine))
            : new DataExportFile(
                $"audit-log-{stamp}.pdf",
                "application/pdf",
                SimplePdf.Create("Waypoint audit log", rows));
        await _audit.LogAsync(
            "DataExported",
            _currentUser.UserId,
            "AuditLog",
            details: $"format={format};action={request.Action};actorUserId={request.ActorUserId};from={request.From:yyyy-MM-dd};to={request.To:yyyy-MM-dd}",
            cancellationToken: cancellationToken);
        return file;
    }
}
