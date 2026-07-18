using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.People;

public sealed class AssignMentorCommandHandler : IRequestHandler<AssignMentorCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public AssignMentorCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(AssignMentorCommand request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(user => user.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
        if (youth.Role != Roles.Youth || youth.Status != UserStatus.Active)
        {
            throw new ConflictException("Choose an active youth for mentor assignment.");
        }

        var mentorExists = await _db.Users.AnyAsync(
            user => user.Id == request.MentorId
                && user.Role == Roles.Mentor
                && user.Status == UserStatus.Active,
            cancellationToken);
        if (!mentorExists)
        {
            throw new ConflictException("Choose an active mentor.");
        }

        youth.MentorId = request.MentorId;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "MentorAssigned",
            _currentUser.UserId,
            "User",
            youth.Id,
            details: request.MentorId.ToString(),
            cancellationToken: cancellationToken);
    }
}
