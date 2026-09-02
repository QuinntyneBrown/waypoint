using MediatR;

namespace Waypoint.Application.Authentication.Invitations;

public record ResendInvitationCommand(Guid InvitationId) : IRequest, IAtomicOutboxRequest;
