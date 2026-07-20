using MediatR;

namespace WordUp.Application.Authentication.Invitations;

public record BulkInviteCommand(
    IReadOnlyList<string> Emails,
    string Role,
    Guid? CohortId,
    Guid? MentorId) : IRequest<BulkInviteResult>, IAtomicOutboxRequest;
