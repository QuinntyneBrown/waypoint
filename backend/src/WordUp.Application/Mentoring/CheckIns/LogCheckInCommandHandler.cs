using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.CheckIns;

public class LogCheckInCommandHandler : IRequestHandler<LogCheckInCommand, CheckInResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public LogCheckInCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<CheckInResult> Handle(LogCheckInCommand request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // No-IDOR: a mentor may only log a check-in for a youth assigned to them; an admin may log for any youth (L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var checkIn = new CheckIn
        {
            YouthUserId = request.YouthId,
            MentorUserId = _currentUser.UserId!.Value,
            Date = request.Date,
            Type = request.Type,
            Engagement = request.Engagement,
            GoalsDiscussed = request.GoalsDiscussed,
            PrayerRequests = request.PrayerRequests,
            ActionItems = request.ActionItems,
            NextFollowUp = request.NextFollowUp,
            // Private by default; only shared with youth/guardian when explicitly enabled (MENTOR-006 AC2).
            Visibility = request.ShareWithYouthAndGuardian
                ? Visibility.SharedWithMentor
                : Visibility.Private,
            GuardianInformed = request.GuardianInformed,
            CreatedAt = _clock.UtcNow,
        };

        _db.CheckIns.Add(checkIn);
        await _db.SaveChangesAsync(cancellationToken);

        // The check-in note content is private (L2-CONSENT-003); never pass note content to the audit log.
        await _audit.LogAsync("CheckInLogged", _currentUser.UserId, "CheckIn", checkIn.Id, isPrivate: true, cancellationToken: cancellationToken);

        return new CheckInResult(checkIn.Id);
    }
}
