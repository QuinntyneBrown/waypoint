using MediatR;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Authentication.Invitations;

public record RevokeInvitationCommand(Guid InvitationId) : IRequest, IAtomicRequest;
