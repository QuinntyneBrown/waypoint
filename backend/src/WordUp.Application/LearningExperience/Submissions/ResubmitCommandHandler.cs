using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.ConsentAndSafeguarding;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Submissions;

public class ResubmitCommandHandler : IRequestHandler<ResubmitCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ResubmitCommandHandler(
        IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
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
        _db.SubmissionTimelineEvents.Add(new SubmissionTimelineEvent
        {
            SubmissionId = submission.Id,
            Type = "Resubmitted",
            OccurredAt = submission.SubmittedAt,
        });
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("SubmissionResubmitted", _currentUser.UserId, "Submission", submission.Id);
    }
}
