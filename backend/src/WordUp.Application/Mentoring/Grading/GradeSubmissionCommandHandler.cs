using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Grading;

public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public GradeSubmissionCommandHandler(
        IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");

        var youth = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == submission.YouthUserId, cancellationToken)
            ?? throw new NotFoundException("Submission owner not found.");

        // Enforce mentor-assignment: a mentor may only grade their own assigned youth; an
        // administrator may grade anyone (L2-MENTOR-003 AC5, L2-AUTH-010 no-IDOR).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var now = _clock.UtcNow;

        switch (request.Decision)
        {
            case "approve":
                submission.Status = SubmissionStatus.Approved;
                submission.RubricScoresJson = request.RubricScoresJson;
                submission.TotalAwarded = request.TotalAwarded;
                submission.MentorComment = request.Comment;
                submission.GuardianNotified = request.GuardianNotified;
                submission.ReviewedByUserId = _currentUser.UserId;
                submission.ReviewedAt = now;
                break;

            case "request-revision":
                submission.Status = SubmissionStatus.NeedsRevision;
                submission.RubricScoresJson = request.RubricScoresJson;
                submission.TotalAwarded = request.TotalAwarded;
                submission.MentorComment = request.Comment;
                submission.GuardianNotified = request.GuardianNotified;
                submission.ReviewedByUserId = _currentUser.UserId;
                submission.ReviewedAt = now;
                break;

            case "save-draft":
                // Store the in-progress grade but do NOT change the youth-visible status
                // (L2-MENTOR-003 AC4).
                submission.RubricScoresJson = request.RubricScoresJson;
                submission.TotalAwarded = request.TotalAwarded;
                submission.MentorComment = request.Comment;
                break;

            default:
                throw new ConflictException("Unknown grading decision.");
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "SubmissionGraded",
            _currentUser.UserId,
            "Submission",
            submission.Id,
            details: request.Decision);
    }
}
