using MediatR;

namespace WordUp.Application.Auth;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, AuthResult>
{
    private readonly ISessionIssuer _session;

    public RefreshCommandHandler(ISessionIssuer session) => _session = session;

    public Task<AuthResult> Handle(RefreshCommand request, CancellationToken cancellationToken) =>
        _session.RefreshAsync(request.RefreshToken, cancellationToken);
}
