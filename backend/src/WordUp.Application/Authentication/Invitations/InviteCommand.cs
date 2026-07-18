using MediatR;

namespace WordUp.Application.Authentication.Invitations;

public record InviteCommand(string Email, string Role, Guid? CohortId, Guid? MentorId) : IRequest<InviteResult>;
