using MediatR;

namespace WordUp.Application.Authentication.Invitations;

public record RevokeInvitationCommand(Guid InvitationId) : IRequest;
