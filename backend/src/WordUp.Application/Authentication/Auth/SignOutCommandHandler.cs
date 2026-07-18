using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Authentication.Auth;

public class SignOutCommandHandler : IRequestHandler<SignOutCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly ISessionIssuer _session;
    private readonly IAuditLogger _audit;

    public SignOutCommandHandler(ICurrentUser currentUser, ISessionIssuer session, IAuditLogger audit)
    {
        _currentUser = currentUser;
        _session = session;
        _audit = audit;
    }

    public async Task Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return;
        }

        await _session.RevokeAllAsync(userId.Value, cancellationToken);
        await _audit.LogAsync("SignOut", userId, "User", userId);
    }
}
