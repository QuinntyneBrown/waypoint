using MediatR;

namespace WordUp.Application.Invitations;

public record ResendInvitationCommand(Guid InvitationId) : IRequest;
