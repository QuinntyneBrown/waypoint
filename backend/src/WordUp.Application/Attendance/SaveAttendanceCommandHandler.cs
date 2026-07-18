using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Attendance;

public class SaveAttendanceCommandHandler : IRequestHandler<SaveAttendanceCommand, AttendanceTally>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public SaveAttendanceCommandHandler(
        IAppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IAuditLogger audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<AttendanceTally> Handle(SaveAttendanceCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session not found.");

        var recordedBy = _currentUser.UserId!.Value;
        var now = _clock.UtcNow;

        // Parse statuses and enforce the volunteer restriction up-front so nothing is written on rejection
        // (volunteers may only mark Present/Absent — L2-ENGAGE-002).
        var parsed = new List<(Guid YouthId, AttendanceStatus Status, string? Note)>();
        foreach (var mark in request.Marks)
        {
            var status = Enum.Parse<AttendanceStatus>(mark.Status, ignoreCase: true);
            if (_currentUser.Role == Roles.Volunteer && status is AttendanceStatus.Late or AttendanceStatus.Excused)
            {
                throw new ForbiddenException("Volunteers may only mark youth Present or Absent.");
            }
            parsed.Add((mark.YouthId, status, mark.Note));
        }

        var youthIds = parsed.Select(p => p.YouthId).ToList();
        var existing = await _db.AttendanceRecords
            .Where(r => r.SessionId == request.SessionId && youthIds.Contains(r.YouthUserId))
            .ToListAsync(cancellationToken);

        // Upsert by (SessionId, YouthUserId): re-saving updates the existing row rather than inserting a
        // duplicate, which is what makes offline sync / re-save idempotent (L2-ENGAGE-003 AC2).
        foreach (var (youthId, status, note) in parsed)
        {
            var record = existing.FirstOrDefault(r => r.YouthUserId == youthId);
            if (record is null)
            {
                record = new AttendanceRecord
                {
                    SessionId = request.SessionId,
                    YouthUserId = youthId,
                    Status = status,
                    Note = note,
                    RecordedByUserId = recordedBy,
                    RecordedAt = now,
                };
                _db.AttendanceRecords.Add(record);
                existing.Add(record); // guard against the same youth appearing twice in one request
            }
            else
            {
                record.Status = status;
                record.Note = note;
                record.RecordedByUserId = recordedBy;
                record.RecordedAt = now;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("AttendanceSaved", recordedBy, "Session", request.SessionId, cancellationToken: cancellationToken);

        // Tally the whole session (every saved mark), not just this request's marks.
        var all = await _db.AttendanceRecords
            .Where(r => r.SessionId == request.SessionId)
            .Select(r => r.Status)
            .ToListAsync(cancellationToken);

        var present = all.Count(s => s == AttendanceStatus.Present);
        var late = all.Count(s => s == AttendanceStatus.Late);
        var excused = all.Count(s => s == AttendanceStatus.Excused);
        var absent = all.Count(s => s == AttendanceStatus.Absent);
        var marked = all.Count;

        var total = await _db.Users
            .CountAsync(u => u.CohortId == session.CohortId && u.Role == Roles.Youth, cancellationToken);

        return new AttendanceTally(present, late, excused, absent, marked, total);
    }
}
