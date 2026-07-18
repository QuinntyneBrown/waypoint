using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Reports;

public record YouthReportQuery(Guid YouthId) : IRequest<YouthReport>;

public record YouthReport(
    Guid YouthId,
    int LessonsCompleted,
    double AvgQuizScore,
    int AttendancePresent,
    int AttendanceTotal,
    int Badges);

public class YouthReportQueryHandler : IRequestHandler<YouthReportQuery, YouthReport>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public YouthReportQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<YouthReport> Handle(YouthReportQuery request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // Scope (L2-REPORT-001 AC6 / L2-AUTH-010): a mentor may only view a youth assigned to them;
        // a program administrator may view any youth.
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var lessonsCompleted = await _db.LessonProgress
            .CountAsync(p => p.YouthUserId == youth.Id && p.Status == ProgressStatus.Completed, cancellationToken);

        var quizScores = await _db.QuizAttempts
            .Where(q => q.YouthUserId == youth.Id)
            .Select(q => q.ApprovedScore ?? q.AutoScore)
            .ToListAsync(cancellationToken);
        var avgQuizScore = quizScores.Count == 0 ? 0d : quizScores.Average();

        var attendance = await _db.AttendanceRecords
            .Where(a => a.YouthUserId == youth.Id)
            .Select(a => a.Status)
            .ToListAsync(cancellationToken);
        var attendanceTotal = attendance.Count;
        var attendancePresent = attendance.Count(s => s == AttendanceStatus.Present || s == AttendanceStatus.Late);

        var badges = await _db.BadgeAwards.CountAsync(b => b.YouthUserId == youth.Id, cancellationToken);

        return new YouthReport(youth.Id, lessonsCompleted, avgQuizScore, attendancePresent, attendanceTotal, badges);
    }
}
