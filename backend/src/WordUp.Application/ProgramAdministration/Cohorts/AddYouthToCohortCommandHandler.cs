using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public class AddYouthToCohortCommandHandler : IRequestHandler<AddYouthToCohortCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public AddYouthToCohortCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(AddYouthToCohortCommand request, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.Id == request.CohortId, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");

        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        youth.CohortId = cohort.Id;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("CohortYouthAdded", _currentUser.UserId, "Cohort", cohort.Id);
    }
}
