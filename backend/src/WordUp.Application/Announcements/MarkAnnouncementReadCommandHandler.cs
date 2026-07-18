using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Announcements;

public class MarkAnnouncementReadCommandHandler : IRequestHandler<MarkAnnouncementReadCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public MarkAnnouncementReadCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(MarkAnnouncementReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var alreadyRead = await _db.AnnouncementReads
            .AnyAsync(ar => ar.AnnouncementId == request.AnnouncementId && ar.UserId == userId, cancellationToken);

        if (alreadyRead)
        {
            return;
        }

        _db.AnnouncementReads.Add(new AnnouncementRead
        {
            AnnouncementId = request.AnnouncementId,
            UserId = userId,
            ReadAt = _clock.UtcNow,
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
