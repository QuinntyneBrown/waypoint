using MediatR;
using WordUp.Application.Auth;

namespace WordUp.Application.Invitations;

public record AcceptInviteCommand(string Token, string DisplayName, string Password, bool AgreeToGuidelines)
    : IRequest<AuthResult>;
