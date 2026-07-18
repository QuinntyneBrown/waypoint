using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Rsvps;

public class SubmitRsvpCommandHandler : IRequestHandler<SubmitRsvpCommand, RsvpResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmitRsvpCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<RsvpResult> Handle(SubmitRsvpCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId!.Value;
        // The validator guarantees the response parses; re-parse here for the canonical value.
        SubmitRsvpCommand.TryParseResponse(request.Response, out var response);

        // Upsert by (session, youth) — re-submitting updates rather than creating a duplicate (ENGAGE-005 AC).
        var rsvp = await _db.SessionRsvps
            .FirstOrDefaultAsync(r => r.SessionId == request.SessionId && r.YouthUserId == youthId, cancellationToken);

        if (rsvp is null)
        {
            rsvp = new SessionRsvp
            {
                SessionId = request.SessionId,
                YouthUserId = youthId,
                Response = response,
                Note = request.Note,
                ReminderSet = request.ReminderSet,
                CreatedAt = _clock.UtcNow,
                UpdatedAt = _clock.UtcNow,
            };
            _db.SessionRsvps.Add(rsvp);
        }
        else
        {
            rsvp.Response = response;
            rsvp.Note = request.Note;
            rsvp.ReminderSet = request.ReminderSet;
            rsvp.UpdatedAt = _clock.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new RsvpResult(rsvp.Id);
    }
}
