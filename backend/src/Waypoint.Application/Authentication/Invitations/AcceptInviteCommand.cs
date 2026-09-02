using MediatR;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Authentication.Invitations;

public record AcceptInviteCommand(string Token, string DisplayName, string Password, bool AgreeToGuidelines)
    : IRequest<AuthResult>, IAtomicRequest;
