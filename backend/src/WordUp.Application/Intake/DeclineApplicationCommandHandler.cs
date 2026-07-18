using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Intake;

public class DeclineApplicationCommandHandler : IRequestHandler<DeclineApplicationCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public DeclineApplicationCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(DeclineApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        // Declining creates no account (L2-INTAKE-002 AC4).
        application.Status = ApplicationStatus.Declined;
        application.DecisionNote = request.DecisionNote?.Trim();
        application.ReviewedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ApplicationDeclined", _currentUser.UserId, "YouthApplication", application.Id);
    }
}
