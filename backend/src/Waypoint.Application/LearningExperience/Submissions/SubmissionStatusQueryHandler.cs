using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Waypoint.Application.ConsentAndSafeguarding;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.LearningExperience.Submissions;

public class SubmissionStatusQueryHandler : IRequestHandler<SubmissionStatusQuery, SubmissionStatusView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmissionStatusQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<SubmissionStatusView> Handle(SubmissionStatusQuery request, CancellationToken cancellationToken)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");

        // Owner-only: a youth may only view their own submission (L2-LEARN-007, no-IDOR).
        if (submission.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("You may only view your own submissions.");
        }
        var assignment = await _db.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == submission.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("Assignment not found.");
        var storedEvents = await _db.SubmissionTimelineEvents.AsNoTracking()
            .Where(item => item.SubmissionId == submission.Id)
            .OrderBy(item => item.OccurredAt)
            .ToListAsync(cancellationToken);
        if (storedEvents.Count == 0 || storedEvents[0].Type != "Submitted")
        {
            storedEvents.Insert(0, new SubmissionTimelineEvent
            {
                SubmissionId = submission.Id,
                Type = "Submitted",
                OccurredAt = submission.CreatedAt,
            });
        }
        var timeline = new List<SubmissionTimelineItem>();
        foreach (var item in storedEvents)
        {
            var label = item.Type switch
            {
                "NeedsRevision" => "Needs revision",
                "Resubmitted" => "Resubmitted",
                "Approved" => "Approved",
                _ => "Submitted",
            };
            timeline.Add(new(item.Type, label, item.OccurredAt, false));
            if (item.Type is "Submitted" or "Resubmitted")
            {
                timeline.Add(new("UnderReview", "Under review", item.OccurredAt, false));
            }
        }
        if (timeline.Count > 0)
        {
            var currentType = submission.Status.ToString();
            var currentIndex = timeline.FindLastIndex(item => item.Type == currentType);
            if (currentIndex < 0 && submission.Status == SubmissionStatus.Submitted)
                currentIndex = timeline.FindLastIndex(item => item.Type == "Submitted");
            if (currentIndex >= 0) timeline[currentIndex] = timeline[currentIndex] with { Current = true };
        }
        var scoreMap = ParseScores(submission.RubricScoresJson);
        var rubric = await _db.RubricCriteria.AsNoTracking()
            .Where(item => item.AssignmentId == assignment.Id)
            .OrderBy(item => item.Order)
            .Select(item => new SubmissionRubricScore(
                item.Id,
                item.Description,
                item.Points,
                scoreMap.ContainsKey(item.Id) ? scoreMap[item.Id] : null))
            .ToListAsync(cancellationToken);
        var badge = await (
            from award in _db.BadgeAwards.AsNoTracking()
            join definition in _db.Badges.AsNoTracking() on award.BadgeId equals definition.Id
            where award.YouthUserId == submission.YouthUserId
                && definition.AutoRule == "assignment-approved"
                && definition.TargetRefId == assignment.Id
                && definition.ArchivedAt == null
            select new SubmissionBadgeView(
                definition.Id,
                definition.Name,
                definition.Icon,
                definition.Description,
                award.AwardedAt))
            .FirstOrDefaultAsync(cancellationToken);
        var photoUrl = submission.PhotoUrl;
        if (photoUrl is not null
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                submission.YouthUserId,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            photoUrl = null;
        }
        SubmissionAttachmentView? file = null;
        if (submission.FileUploadId is { } fileId)
        {
            file = await _db.StoredSubmissionFiles.AsNoTracking()
                .Where(item => item.Id == fileId && item.Kind == SubmissionFileKind.File)
                .Select(item => new SubmissionAttachmentView(
                    item.Id,
                    item.OriginalFileName,
                    item.ContentType,
                    item.SizeBytes,
                    $"/api/submission-files/{item.Id}"))
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new SubmissionStatusView(
            submission.Id,
            submission.AssignmentId,
            assignment.Title,
            submission.Status.ToString(),
            submission.ConcurrencyStamp.ToString(),
            submission.IsCodingProject,
            submission.BodyText,
            submission.YouthNote,
            photoUrl,
            submission.LinkUrl,
            file,
            submission.MentorComment,
            submission.TotalAwarded,
            assignment.TotalPoints,
            submission.SubmittedAt,
            submission.ReviewedAt,
            timeline,
            rubric,
            badge);
    }

    private static IReadOnlyDictionary<Guid, int> ParseScores(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<Guid, int>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateObject()
                .Where(item => Guid.TryParse(item.Name, out _) && item.Value.TryGetInt32(out _))
                .ToDictionary(
                    item => Guid.Parse(item.Name),
                    item => item.Value.GetInt32());
        }
        catch (JsonException)
        {
            return new Dictionary<Guid, int>();
        }
    }
}
