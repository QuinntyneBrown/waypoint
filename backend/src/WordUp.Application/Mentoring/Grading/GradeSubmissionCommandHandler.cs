using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
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

        if (request.ExpectedVersion is not null
            && (!Guid.TryParse(request.ExpectedVersion, out var expectedVersion)
                || expectedVersion != submission.ConcurrencyStamp))
        {
            throw new ConflictException(
                "This submission changed after you opened it. Review the latest version before saving again.");
        }

        var criteria = await _db.RubricCriteria
            .Where(item => item.AssignmentId == submission.AssignmentId)
            .ToListAsync(cancellationToken);
        var scores = ParseScores(request.RubricScoresJson);
        var criteriaById = criteria.ToDictionary(item => item.Id);
        var failures = scores
            .Where(score => !criteriaById.TryGetValue(score.Key, out var criterion)
                || score.Value < 0
                || score.Value > criterion.Points)
            .Select(score => new ValidationFailure(
                nameof(request.RubricScoresJson),
                $"Score for criterion {score.Key} is outside its allowed range."))
            .ToList();
        if (request.Decision == "approve")
        {
            failures.AddRange(criteria
                .Where(criterion => !scores.ContainsKey(criterion.Id))
                .Select(criterion => new ValidationFailure(
                    nameof(request.RubricScoresJson),
                    $"A score is required for criterion {criterion.Id} before approval.")));
        }

        if (failures.Count > 0) throw new ValidationException(failures);
        var normalizedScores = scores.Count == 0 ? null : JsonSerializer.Serialize(scores);
        int? computedTotal = scores.Count == 0 ? null : scores.Values.Sum();
        var now = _clock.UtcNow;
        var badgeNotifications = new List<Notification>();

        switch (request.Decision)
        {
            case "approve":
                submission.Status = SubmissionStatus.Approved;
                submission.RubricScoresJson = normalizedScores;
                submission.TotalAwarded = computedTotal;
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
                        var badgeNotification = new Notification
                        {
                            UserId = youth.Id,
                            EventKey = "badge-earned",
                            Title = $"Badge earned: {badge.Name}",
                            Body = string.IsNullOrWhiteSpace(badge.Description)
                                ? "You earned a new badge."
                                : badge.Description,
                            LinkPath = "/youth/journey",
                            CreatedAt = now,
                        };
                        _db.Notifications.Add(badgeNotification);
                        badgeNotifications.Add(badgeNotification);
                    }
                }
                break;

            case "request-revision":
                submission.Status = SubmissionStatus.NeedsRevision;
                submission.RubricScoresJson = normalizedScores;
                submission.TotalAwarded = computedTotal;
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
                submission.RubricScoresJson = normalizedScores;
                submission.TotalAwarded = computedTotal;
                submission.MentorComment = request.Comment;
                break;

            default:
                throw new ConflictException("Unknown grading decision.");
        }

        submission.ConcurrencyStamp = Guid.NewGuid();
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "This submission changed after you opened it. Review the latest version before saving again.");
        }

        if (request.Decision is "approve" or "request-revision")
        {
            var youthNotification = new Notification
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
            var notifications = new List<Notification> { youthNotification };
            if (request.GuardianNotified && youth.GuardianUserId is Guid guardianUserId)
            {
                notifications.Add(new Notification
                {
                    UserId = guardianUserId,
                    EventKey = "submission-status",
                    Title = request.Decision == "approve"
                        ? $"{youth.DisplayName}'s assignment was approved"
                        : $"{youth.DisplayName}'s assignment needs revision",
                    Body = request.Decision == "approve"
                        ? "Their mentor approved the submitted work."
                        : "Their mentor requested a revision and shared feedback.",
                    LinkPath = "/guardian/progress",
                    CreatedAt = now,
                });
            }

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync(cancellationToken);
            foreach (var notification in badgeNotifications.Concat(notifications))
            {
                await _publisher.PublishToUserAsync(
                    notification.UserId,
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
        }

        await _audit.LogAsync(
            "SubmissionGraded",
            _currentUser.UserId,
            "Submission",
            submission.Id,
            details: request.Decision);
    }

    private static IReadOnlyDictionary<Guid, int> ParseScores(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<Guid, int>();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<Guid, int>>(json)
                ?? new Dictionary<Guid, int>();
        }
        catch (JsonException)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(GradeSubmissionCommand.RubricScoresJson), "Rubric scores are invalid.")
            ]);
        }
    }
}
