using MediatR;

namespace WordUp.Application.Authentication.Invitations;

public record ResendInvitationCommand(Guid InvitationId) : IRequest;
