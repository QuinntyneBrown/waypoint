using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.Privacy;

public class MyDataQueryHandler : IRequestHandler<MyDataQuery, MyDataResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MyDataQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MyDataResult> Handle(MyDataQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        // Access is scoped strictly to the caller's own record — no other subject's data is reachable here.
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return new MyDataResult(
            user.Id,
            user.DisplayName,
            user.Email,
            user.Role,
            user.Status.ToString(),
            user.Pronouns,
            user.PhotoUrl,
            user.CohortId,
            user.MentorId,
            user.GuardianUserId,
            user.CreatedAt,
            user.LastSignInAt);
    }
}
