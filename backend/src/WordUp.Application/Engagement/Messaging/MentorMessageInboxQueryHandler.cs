using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Messaging;

public sealed class MentorMessageInboxQueryHandler
    : IRequestHandler<MentorMessageInboxQuery, MentorMessageInboxView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MentorMessageInboxQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MentorMessageInboxView> Handle(
        MentorMessageInboxQuery request,
        CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId ?? throw new ForbiddenException();
        var youth = await _db.Users.AsNoTracking()
            .Where(user => user.Role == Roles.Youth
                && user.Status == UserStatus.Active
                && user.MentorId == mentorId)
            .Select(user => new { user.Id, user.DisplayName, user.CohortId })
            .OrderBy(user => user.DisplayName)
            .ToListAsync(cancellationToken);
        var youthIds = youth.Select(user => user.Id).ToList();
        var cohortIds = youth
            .Where(user => user.CohortId != null)
            .Select(user => user.CohortId!.Value)
            .Distinct()
            .ToList();
        var cohortNames = await _db.Cohorts.AsNoTracking()
            .Where(cohort => cohortIds.Contains(cohort.Id))
            .ToDictionaryAsync(cohort => cohort.Id, cohort => cohort.Name, cancellationToken);
        var messages = await _db.Messages.AsNoTracking()
            .Where(message => youthIds.Contains(message.SenderId)
                || youthIds.Contains(message.RecipientId))
            .OrderBy(message => message.SentAt)
            .ToListAsync(cancellationToken);

        var conversations = youth.Select(person =>
        {
            var threadKey = Message.KeyFor(mentorId, person.Id);
            var thread = messages.Where(message => message.ThreadKey == threadKey).ToList();
            var last = thread.LastOrDefault();
            var decoded = last is null ? null : WebUtility.HtmlDecode(last.Body);
            var preview = decoded is { Length: > 100 } ? $"{decoded[..100]}…" : decoded;
            return new MentorConversationView(
                person.Id,
                person.DisplayName,
                person.CohortId,
                person.CohortId is { } cohortId
                    ? cohortNames.GetValueOrDefault(cohortId)
                    : null,
                thread.Count(message => message.RecipientId == mentorId && message.ReadAt == null),
                preview,
                last?.SentAt);
        }).ToList();

        var groups = youth
            .Where(person => person.CohortId != null)
            .GroupBy(person => person.CohortId!.Value)
            .Where(group => cohortNames.ContainsKey(group.Key))
            .OrderBy(group => cohortNames[group.Key])
            .Select(group => new MentorBroadcastGroupView(
                group.Key,
                cohortNames[group.Key],
                group.Count()))
            .ToList();

        return new MentorMessageInboxView(conversations, groups);
    }
}
