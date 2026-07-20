using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Authentication.Invitations;

public record RevokeInvitationCommand(Guid InvitationId) : IRequest, IAtomicRequest;
