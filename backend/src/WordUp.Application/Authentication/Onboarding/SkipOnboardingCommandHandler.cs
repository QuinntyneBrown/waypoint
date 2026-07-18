using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Authentication.Onboarding;

public class SkipOnboardingCommandHandler : IRequestHandler<SkipOnboardingCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public SkipOnboardingCommandHandler(ICurrentUser currentUser, IAuditLogger audit)
    {
        _currentUser = currentUser;
        _audit = audit;
    }

    public Task Handle(SkipOnboardingCommand request, CancellationToken cancellationToken) =>
        _audit.LogAsync("OnboardingSkipped", _currentUser.UserId, "User", _currentUser.UserId,
            cancellationToken: cancellationToken);
}
