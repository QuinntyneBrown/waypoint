using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ReportingAndData.DataTransfer;

public record ExportAuditLogQuery() : IRequest<CsvFile>;

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
