using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Sessions;

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, CreateSessionResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public CreateSessionCommandHandler(
        IAppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IAuditLogger audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<CreateSessionResult> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.Id == request.CohortId, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");

        var createdBy = _currentUser.UserId!.Value;
        var mentorId = request.MentorId!.Value;
        var mentorExists = await _db.Users.AnyAsync(
            user => user.Id == mentorId
                && user.Role == Roles.Mentor
                && user.Status == UserStatus.Active,
            cancellationToken);
        if (!mentorExists) throw new NotFoundException("Active mentor not found.");
        if (_currentUser.Role == Roles.Mentor)
        {
            if (mentorId != createdBy)
            {
                throw new ForbiddenException("Mentors may only schedule sessions assigned to themselves.");
            }

            var hasCohortAccess = cohort.MentorId == createdBy
                || await _db.CohortMentors.AnyAsync(
                    assignment => assignment.CohortId == cohort.Id
                        && assignment.MentorId == createdBy,
                    cancellationToken)
                || await _db.Users.AnyAsync(
                    user => user.CohortId == cohort.Id
                        && user.MentorId == createdBy
                        && user.Role == Roles.Youth
                        && user.Status == UserStatus.Active,
                    cancellationToken);
            if (!hasCohortAccess)
            {
                throw new ForbiddenException("This cohort is not assigned to you.");
            }
        }
        if (request.StartsAt < cohort.TermStart || request.StartsAt > cohort.TermEnd)
        {
            throw new ValidationException("The session must fall within the cohort term.");
        }

        var now = _clock.UtcNow;
        var sessions = new List<Session>();

        if (request.RepeatsWeekly)
        {
            // Materialise one row per week from the start date, inclusive through the term-end date,
            // never beyond it; all generated rows share a single recurrence group id (L2-ENGAGE-001 AC2).
            var groupId = Guid.NewGuid();
            for (var starts = request.StartsAt; starts <= cohort.TermEnd; starts = starts.AddDays(7))
            {
                sessions.Add(NewSession(request, starts, createdBy, now, repeatsWeekly: true, groupId));
            }
        }
        else
        {
            sessions.Add(NewSession(request, request.StartsAt, createdBy, now, repeatsWeekly: false, recurrenceGroupId: null));
        }

        _db.Sessions.AddRange(sessions);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("SessionCreated", createdBy, "Session", sessions[0].Id, cancellationToken: cancellationToken);

        return new CreateSessionResult(sessions[0].Id, sessions.Count);
    }

    private static Session NewSession(
        CreateSessionCommand request,
        DateTime startsAt,
        Guid createdBy,
        DateTime now,
        bool repeatsWeekly,
        Guid? recurrenceGroupId) => new()
        {
            CohortId = request.CohortId,
            MentorId = request.MentorId,
            Title = request.Title,
            StartsAt = startsAt,
            DurationMinutes = request.DurationMinutes,
            Location = request.Location,
            RepeatsWeekly = repeatsWeekly,
            RecurrenceGroupId = recurrenceGroupId,
            CreatedByUserId = createdBy,
            CreatedAt = now,
        };
}
