using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Rsvps;

public class SessionRsvpsQueryHandler : IRequestHandler<SessionRsvpsQuery, IReadOnlyList<RsvpView>>
{
    private readonly IAppDbContext _db;

    public SessionRsvpsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<RsvpView>> Handle(SessionRsvpsQuery request, CancellationToken cancellationToken)
    {
        return await _db.SessionRsvps
            .Where(r => r.SessionId == request.SessionId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new RsvpView(r.YouthUserId, r.Response.ToString(), r.Note, r.ReminderSet))
            .ToListAsync(cancellationToken);
    }
}
