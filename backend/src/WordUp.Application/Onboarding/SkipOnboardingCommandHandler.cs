using MediatR;
using WordUp.Application.Abstractions;

namespace WordUp.Application.Onboarding;

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
