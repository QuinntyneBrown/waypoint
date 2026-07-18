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
    private readonly IChangePublisher _publisher;

    public GradeSubmissionCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IChangePublisher publisher)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _publisher = publisher;
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
                _db.SubmissionTimelineEvents.Add(new SubmissionTimelineEvent
                {
                    SubmissionId = submission.Id,
                    Type = "Approved",
                    OccurredAt = now,
                });
                var badges = await _db.Badges
                    .Where(item => item.AutoRule == "assignment-approved"
                        && item.TargetRefId == submission.AssignmentId
                        && item.ArchivedAt == null)
                    .ToListAsync(cancellationToken);
                foreach (var badge in badges)
                {
                    if (!await _db.BadgeAwards.AnyAsync(item =>
                            item.BadgeId == badge.Id && item.YouthUserId == youth.Id,
                            cancellationToken))
                    {
                        _db.BadgeAwards.Add(new BadgeAward
                        {
                            BadgeId = badge.Id,
                            YouthUserId = youth.Id,
                            AwardedByUserId = _currentUser.UserId,
                            AwardedAt = now,
                        });
                    }
                }
                break;

            case "request-revision":
                submission.Status = SubmissionStatus.NeedsRevision;
                submission.RubricScoresJson = request.RubricScoresJson;
                submission.TotalAwarded = request.TotalAwarded;
                submission.MentorComment = request.Comment;
                submission.GuardianNotified = request.GuardianNotified;
                submission.ReviewedByUserId = _currentUser.UserId;
                submission.ReviewedAt = now;
                _db.SubmissionTimelineEvents.Add(new SubmissionTimelineEvent
                {
                    SubmissionId = submission.Id,
                    Type = "NeedsRevision",
                    OccurredAt = now,
                });
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

        if (request.Decision is "approve" or "request-revision")
        {
            var notification = new Notification
            {
                UserId = youth.Id,
                EventKey = "submission-status",
                Title = request.Decision == "approve" ? "Assignment approved" : "Assignment needs revision",
                Body = request.Decision == "approve"
                    ? "Your mentor approved your assignment."
                    : "Your mentor left revision feedback.",
                LinkPath = $"/youth/submissions/{submission.Id}",
                CreatedAt = now,
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(cancellationToken);
            await _publisher.PublishToUserAsync(
                youth.Id,
                "notification",
                new
                {
                    id = notification.Id,
                    eventKey = notification.EventKey,
                    title = notification.Title,
                    body = notification.Body,
                    linkPath = notification.LinkPath,
                    createdAt = notification.CreatedAt,
                },
                cancellationToken);
        }

        await _audit.LogAsync(
            "SubmissionGraded",
            _currentUser.UserId,
            "Submission",
            submission.Id,
            details: request.Decision);
    }
}
