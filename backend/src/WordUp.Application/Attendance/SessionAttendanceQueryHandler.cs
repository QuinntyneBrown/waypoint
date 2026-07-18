using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;

namespace WordUp.Application.Attendance;

public class SessionAttendanceQueryHandler : IRequestHandler<SessionAttendanceQuery, IReadOnlyList<AttendanceMarkView>>
{
    private readonly IAppDbContext _db;

    public SessionAttendanceQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<AttendanceMarkView>> Handle(SessionAttendanceQuery request, CancellationToken cancellationToken)
    {
        return await _db.AttendanceRecords
            .Where(r => r.SessionId == request.SessionId)
            .OrderBy(r => r.YouthUserId)
            .Select(r => new AttendanceMarkView(r.YouthUserId, r.Status.ToString(), r.Note, r.RecordedAt))
            .ToListAsync(cancellationToken);
    }
}
