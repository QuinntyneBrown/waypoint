using MediatR;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Badges;

public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, BadgeResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public CreateBadgeCommandHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<BadgeResult> Handle(CreateBadgeCommand request, CancellationToken cancellationToken)
    {
        var badge = new Badge
        {
            Name = request.Name.Trim(),
            Icon = request.Icon,
            Description = request.Description,
            CriteriaKind = request.CriteriaKind,
            AutoRule = request.AutoRule,
            CompetencyId = request.CompetencyId,
            CreatedAt = _clock.UtcNow,
        };

        _db.Badges.Add(badge);
        await _db.SaveChangesAsync(cancellationToken);

        return new BadgeResult(badge.Id);
    }
}
