using MediatR;
using WordUp.Application.Authentication.Auth;

namespace WordUp.Application.Authentication.Invitations;

public record AcceptInviteCommand(string Token, string DisplayName, string Password, bool AgreeToGuidelines)
    : IRequest<AuthResult>;
