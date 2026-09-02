using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.ConsentAndSafeguarding;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.LearningExperience.Submissions;

public class ResubmitCommandHandler : IRequestHandler<ResubmitCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly NotificationDeliveryService _notifications;

    public ResubmitCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        NotificationDeliveryService notifications)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(ResubmitCommand request, CancellationToken cancellationToken)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");

        // Owner-only (no-IDOR).
        if (submission.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("You may only resubmit your own submissions.");
        }

        if (request.ExpectedVersion is not null
            && (!Guid.TryParse(request.ExpectedVersion, out var expectedVersion)
                || expectedVersion != submission.ConcurrencyStamp))
        {
            throw new ConflictException(
                "This submission changed after you opened it. Review the latest feedback before resubmitting.");
        }

        // Only a submission that a mentor sent back may be resubmitted (L2-LEARN-007 AC3).
        if (submission.Status != SubmissionStatus.NeedsRevision)
        {
            throw new ConflictException("This submission is not awaiting revisions.");
        }

        if (!string.IsNullOrWhiteSpace(request.PhotoUrl)
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                submission.YouthUserId,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            throw new MediaConsentRequiredException();
        }

        if (request.BodyText is not null)
        {
            submission.BodyText = request.BodyText;
        }
        if (request.PhotoUrl is not null)
        {
            submission.PhotoUrl = request.PhotoUrl;
        }
        if (request.LinkUrl is not null)
        {
            submission.LinkUrl = request.LinkUrl;
        }

        submission.Status = SubmissionStatus.UnderReview;
        submission.SubmittedAt = _clock.UtcNow;
        submission.ConcurrencyStamp = Guid.NewGuid();
        _db.SubmissionTimelineEvents.Add(new SubmissionTimelineEvent
        {
            SubmissionId = submission.Id,
            Type = "Resubmitted",
            OccurredAt = submission.SubmittedAt,
        });
        NotificationDeliveryPlan? approvalPlan = null;
        var submitter = await _db.Users.AsNoTracking()
            .Where(item => item.Id == submission.YouthUserId)
            .Select(item => new { item.DisplayName, item.MentorId })
            .SingleAsync(cancellationToken);
        if (submitter.MentorId is { } mentorId)
        {
            approvalPlan = await _notifications.PrepareAsync(
                mentorId,
                "approval-needed",
                "Revised assignment ready for review",
                "A participant resubmitted revised work.",
                $"/mentor/reviews/submissions/{submission.Id}",
                submission.SubmittedAt,
                cancellationToken,
                subjectUserId: submission.YouthUserId);
        }
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "This submission changed after you opened it. Review the latest feedback before resubmitting.");
        }
        if (approvalPlan is not null)
        {
            await _notifications.DispatchAsync(approvalPlan, cancellationToken);
        }

        await _audit.LogAsync("SubmissionResubmitted", _currentUser.UserId, "Submission", submission.Id);
    }
}
