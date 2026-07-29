using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.People;

public sealed class LinkGuardianCommandHandler : IRequestHandler<LinkGuardianCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public LinkGuardianCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(LinkGuardianCommand request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(user => user.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
        if (youth.Role != Roles.Youth || youth.Status != UserStatus.Active)
        {
            throw new ConflictException("Choose an active youth for guardian linkage.");
        }

        var guardianExists = await _db.Users.AnyAsync(
            user => user.Id == request.GuardianId
                && user.Role == Roles.Guardian
                && user.Status == UserStatus.Active,
            cancellationToken);
        if (!guardianExists)
        {
            throw new ConflictException("Choose an active guardian.");
        }

        youth.GuardianUserId = request.GuardianId;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "GuardianLinked",
            _currentUser.UserId,
            "User",
            youth.Id,
            details: request.GuardianId.ToString(),
            cancellationToken: cancellationToken);
    }
}
