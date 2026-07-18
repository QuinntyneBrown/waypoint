using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.AssignWork;

public class AssignWorkCommandHandler : IRequestHandler<AssignWorkCommand, AssignWorkResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public AssignWorkCommandHandler(IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<AssignWorkResult> Handle(AssignWorkCommand request, CancellationToken cancellationToken)
    {
        // Resolve recipients: explicit youth ids, or every youth in the cohort.
        List<User> recipients;
        if (request.YouthIds is { Count: > 0 })
        {
            var ids = request.YouthIds.Distinct().ToList();
            recipients = await _db.Users
                .Where(u => ids.Contains(u.Id) && u.Role == Roles.Youth)
                .ToListAsync(cancellationToken);
        }
        else if (request.CohortId is { } cohortId)
        {
            recipients = await _db.Users
                .Where(u => u.CohortId == cohortId && u.Role == Roles.Youth)
                .ToListAsync(cancellationToken);
        }
        else
        {
            recipients = new List<User>();
        }

        // A mentor may only assign to their assigned youth; a Program Administrator may assign to any (L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && recipients.Any(r => r.MentorId != _currentUser.UserId))
        {
            throw new ForbiddenException("One or more recipients are not assigned to you.");
        }

        var now = _clock.UtcNow;
        var created = 0;
        foreach (var youth in recipients)
        {
            _db.AssignedWork.Add(new AssignedWork
            {
                ItemType = request.ItemType,
                ItemId = request.ItemId,
                YouthUserId = youth.Id,
                CohortId = request.CohortId,
                DueDate = request.DueDate,
                ReminderCadence = request.ReminderCadence,
                Message = request.Message,
                AssignedByUserId = _currentUser.UserId ?? Guid.Empty,
                CreatedAt = now,
            });
            created++;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("WorkAssigned", _currentUser.UserId, request.ItemType, request.ItemId,
            details: $"{created} recipient(s)");

        return new AssignWorkResult(created);
    }
}
