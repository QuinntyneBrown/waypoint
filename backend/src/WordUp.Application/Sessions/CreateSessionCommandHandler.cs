using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Sessions;

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
