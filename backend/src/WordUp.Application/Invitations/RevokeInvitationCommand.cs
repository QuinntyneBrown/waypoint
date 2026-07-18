using MediatR;

namespace WordUp.Application.Invitations;

public record RevokeInvitationCommand(Guid InvitationId) : IRequest;
