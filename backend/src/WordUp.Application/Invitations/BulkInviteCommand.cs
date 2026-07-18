using MediatR;

namespace WordUp.Application.Invitations;

public record BulkInviteCommand(IReadOnlyList<string> Emails, string Role) : IRequest<BulkInviteResult>;
