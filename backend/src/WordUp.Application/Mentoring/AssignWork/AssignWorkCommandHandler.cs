using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.AssignWork;

public class AssignWorkCommandHandler : IRequestHandler<AssignWorkCommand, AssignWorkResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IChangePublisher _publisher;

    public AssignWorkCommandHandler(
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

    public async Task<AssignWorkResult> Handle(AssignWorkCommand request, CancellationToken cancellationToken)
    {
        var recipients = await ResolveRecipientsAsync(request, cancellationToken);
        var item = await ResolveItemAsync(request.ItemType, request.ItemId, cancellationToken);
        if (!await _db.Tracks.AnyAsync(
            track => track.Id == item.TrackId
                && track.Enabled
                && track.Status == PublishStatus.Published,
            cancellationToken))
        {
            throw new ConflictException("This track is paused for new assignments.");
        }

        // A mentor may only assign to their assigned youth; a Program Administrator may assign to any (L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && recipients.Any(r => r.MentorId != _currentUser.UserId))
        {
            throw new ForbiddenException("One or more recipients are not assigned to you.");
        }

        await EnsureItemIsInJourneysAsync(recipients, item.TrackId, cancellationToken);

        var now = _clock.UtcNow;
        var senderId = _currentUser.UserId ?? Guid.Empty;
        var messageBody = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim();
        var reminderCadence = string.IsNullOrWhiteSpace(request.ReminderCadence)
            ? null
            : request.ReminderCadence.Trim().ToLowerInvariant();
        var recipientIds = recipients.Select(recipient => recipient.Id).ToList();
        var messageOptOutIds = messageBody is null
            ? new HashSet<Guid>()
            : (await _db.NotificationPreferences
                .Where(preference => recipientIds.Contains(preference.UserId)
                    && preference.EventKey == "mentor-messages"
                    && !preference.InApp)
                .Select(preference => preference.UserId)
                .ToListAsync(cancellationToken)).ToHashSet();
        var messages = new List<Message>();
        var notifications = new List<Notification>();
        foreach (var youth in recipients)
        {
            var work = new AssignedWork
            {
                ItemType = item.ItemType,
                ItemId = request.ItemId,
                YouthUserId = youth.Id,
                CohortId = request.CohortId,
                DueDate = request.DueDate,
                ReminderCadence = reminderCadence,
                Message = messageBody,
                AssignedByUserId = senderId,
                CreatedAt = now,
            };
            _db.AssignedWork.Add(work);

            if (reminderCadence is not null && request.DueDate is { } dueDate)
            {
                var deliverAt = dueDate - ReminderCadences.Offset(reminderCadence);
                if (deliverAt < now) deliverAt = now;
                _db.ScheduledNotifications.Add(new ScheduledNotification
                {
                    UserId = youth.Id,
                    EventKey = "lesson-reminders",
                    Title = "Work due soon",
                    Body = $"{item.Title} is due {dueDate:MMM d}.",
                    LinkPath = item.LinkPath,
                    SourceType = "AssignedWork",
                    SourceId = work.Id,
                    DeliverAt = deliverAt,
                    CreatedAt = now,
                });
            }

            if (messageBody is null) continue;
            var message = new Message
            {
                ThreadKey = Message.KeyFor(senderId, youth.Id),
                SenderId = senderId,
                RecipientId = youth.Id,
                Body = WebUtility.HtmlEncode(messageBody),
                SentAt = now,
            };
            _db.Messages.Add(message);
            messages.Add(message);
            if (!messageOptOutIds.Contains(youth.Id))
            {
                var notification = new Notification
                {
                    UserId = youth.Id,
                    EventKey = "mentor-messages",
                    Title = "New assigned work",
                    Body = $"Your mentor assigned {item.Title} and sent you a message.",
                    LinkPath = "/youth/messages",
                    CreatedAt = now,
                };
                _db.Notifications.Add(notification);
                notifications.Add(notification);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var message in messages)
        {
            await _publisher.PublishToUserAsync(
                message.RecipientId,
                "message",
                new
                {
                    id = message.Id,
                    senderId = message.SenderId,
                    recipientId = message.RecipientId,
                    body = messageBody,
                    sentAt = message.SentAt,
                    readAt = message.ReadAt,
                },
                cancellationToken);
        }
        foreach (var notification in notifications)
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

        await _audit.LogAsync(
            "WorkAssigned",
            _currentUser.UserId,
            item.ItemType,
            request.ItemId,
            details: $"{recipients.Count} recipient(s)",
            cancellationToken: cancellationToken);

        return new AssignWorkResult(recipients.Count);
    }

    private async Task<List<User>> ResolveRecipientsAsync(
        AssignWorkCommand request,
        CancellationToken cancellationToken)
    {
        if (request.YouthIds is { Count: > 0 })
        {
            var ids = request.YouthIds.Distinct().ToList();
            var recipients = await _db.Users
                .Where(user => ids.Contains(user.Id)
                    && user.Role == Roles.Youth
                    && user.Status == UserStatus.Active)
                .ToListAsync(cancellationToken);
            if (recipients.Count != ids.Count)
            {
                throw new NotFoundException("One or more youth recipients were not found.");
            }
            return recipients;
        }

        var cohortId = request.CohortId!.Value;
        var cohortExists = await _db.Cohorts.AnyAsync(cohort => cohort.Id == cohortId, cancellationToken);
        if (!cohortExists) throw new NotFoundException("Cohort not found.");
        var cohortRecipients = await _db.Users
            .Where(user => user.CohortId == cohortId
                && user.Role == Roles.Youth
                && user.Status == UserStatus.Active)
            .ToListAsync(cancellationToken);
        if (cohortRecipients.Count == 0)
        {
            throw new NotFoundException("The cohort has no active youth.");
        }
        return cohortRecipients;
    }

    private async Task<ResolvedWorkItem> ResolveItemAsync(
        string itemType,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        if (itemType.Equals(AssignWorkItemTypes.Module, StringComparison.OrdinalIgnoreCase))
        {
            var module = await _db.Modules.FirstOrDefaultAsync(
                candidate => candidate.Id == itemId && candidate.Status == PublishStatus.Published,
                cancellationToken) ?? throw new NotFoundException("Published module not found.");
            return new ResolvedWorkItem(
                AssignWorkItemTypes.Module,
                module.Name,
                module.TrackId,
                "/youth/curriculum");
        }

        if (itemType.Equals(AssignWorkItemTypes.Lesson, StringComparison.OrdinalIgnoreCase))
        {
            var lesson = await _db.Lessons.FirstOrDefaultAsync(
                candidate => candidate.Id == itemId
                    && candidate.Status == PublishStatus.Published
                    && candidate.ArchivedAt == null,
                cancellationToken) ?? throw new NotFoundException("Published lesson not found.");
            var module = await _db.Modules.FirstOrDefaultAsync(
                candidate => candidate.Id == lesson.ModuleId
                    && candidate.Status == PublishStatus.Published,
                cancellationToken) ?? throw new NotFoundException("Published lesson not found.");
            return new ResolvedWorkItem(
                AssignWorkItemTypes.Lesson,
                lesson.Title,
                module.TrackId,
                lesson.IsCodingLesson
                    ? $"/youth/coding/{lesson.Id}"
                    : $"/youth/lessons/{lesson.Id}");
        }

        var assignment = await _db.Assignments.FirstOrDefaultAsync(
            candidate => candidate.Id == itemId,
            cancellationToken) ?? throw new NotFoundException("Project not found.");
        var moduleId = assignment.ModuleId;
        if (moduleId is null && assignment.LessonId is { } lessonId)
        {
            moduleId = await _db.Lessons
                .Where(lesson => lesson.Id == lessonId
                    && lesson.Status == PublishStatus.Published
                    && lesson.ArchivedAt == null)
                .Select(lesson => (Guid?)lesson.ModuleId)
                .FirstOrDefaultAsync(cancellationToken);
        }
        var assignmentModule = moduleId is { } id
            ? await _db.Modules.FirstOrDefaultAsync(
                module => module.Id == id && module.Status == PublishStatus.Published,
                cancellationToken)
            : null;
        if (assignmentModule is null) throw new NotFoundException("Published project not found.");
        return new ResolvedWorkItem(
            AssignWorkItemTypes.Project,
            assignment.Title,
            assignmentModule.TrackId,
            $"/youth/assignments/{assignment.Id}");
    }

    private async Task EnsureItemIsInJourneysAsync(
        IReadOnlyCollection<User> recipients,
        Guid trackId,
        CancellationToken cancellationToken)
    {
        var cohortIds = recipients
            .Where(recipient => recipient.CohortId != null)
            .Select(recipient => recipient.CohortId!.Value)
            .Distinct()
            .ToList();
        var cohortTracks = await _db.Cohorts
            .Where(cohort => cohortIds.Contains(cohort.Id) && cohort.TrackId != null)
            .ToDictionaryAsync(cohort => cohort.Id, cohort => cohort.TrackId!.Value, cancellationToken);
        var recipientIds = recipients.Select(recipient => recipient.Id).ToList();
        var additionalEnrollments = await _db.YouthTrackEnrollments
            .Where(enrollment => recipientIds.Contains(enrollment.YouthUserId))
            .Select(enrollment => new { enrollment.YouthUserId, enrollment.TrackId })
            .ToListAsync(cancellationToken);
        if (recipients.Any(recipient =>
            !(recipient.CohortId is { } cohortId
                && cohortTracks.GetValueOrDefault(cohortId) == trackId)
            && !additionalEnrollments.Any(enrollment =>
                enrollment.YouthUserId == recipient.Id && enrollment.TrackId == trackId)))
        {
            throw new ForbiddenException("The selected work is not in every recipient's journey.");
        }
    }

    private sealed record ResolvedWorkItem(
        string ItemType,
        string Title,
        Guid TrackId,
        string LinkPath);
}
