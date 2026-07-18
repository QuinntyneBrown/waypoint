using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.People;

public class AssignMentorCommandHandler : IRequestHandler<AssignMentorCommand>
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
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Person not found.");

        youth.MentorId = request.MentorId;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("MentorAssigned", _currentUser.UserId, "User", youth.Id, details: request.MentorId.ToString());
    }
}
