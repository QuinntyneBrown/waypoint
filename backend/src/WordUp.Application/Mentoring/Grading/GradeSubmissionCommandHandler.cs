using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Grading;

public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly NotificationDeliveryService _notifications;

    public GradeSubmissionCommandHandler(
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
        var badgePlans = new List<NotificationDeliveryPlan>();

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
                        badgePlans.Add(await _notifications.PrepareAsync(
                            youth.Id,
                            "badge-earned",
                            $"Badge earned: {badge.Name}",
                            string.IsNullOrWhiteSpace(badge.Description)
                                ? "You earned a new badge."
                                : badge.Description,
                            "/youth/journey",
                            now,
                            cancellationToken));
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
            var plans = new List<NotificationDeliveryPlan>
            {
                await _notifications.PrepareAsync(
                    youth.Id,
                    "work-graded",
                    request.Decision == "approve" ? "Assignment approved" : "Assignment needs revision",
                    request.Decision == "approve"
                        ? "Your mentor approved your assignment."
                        : "Your mentor left revision feedback.",
                    $"/youth/submissions/{submission.Id}",
                    now,
                    cancellationToken),
            };
            if (request.GuardianNotified && youth.GuardianUserId is Guid guardianUserId)
            {
                plans.Add(await _notifications.PrepareAsync(
                    guardianUserId,
                    "work-graded",
                    request.Decision == "approve"
                        ? $"{youth.DisplayName}'s assignment was approved"
                        : $"{youth.DisplayName}'s assignment needs revision",
                    request.Decision == "approve"
                        ? "Their mentor approved the submitted work."
                        : "Their mentor requested a revision and shared feedback.",
                    "/guardian/progress",
                    now,
                    cancellationToken));
            }

            await _db.SaveChangesAsync(cancellationToken);
            foreach (var plan in badgePlans.Concat(plans))
            {
                await _notifications.DispatchAsync(plan, cancellationToken);
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
