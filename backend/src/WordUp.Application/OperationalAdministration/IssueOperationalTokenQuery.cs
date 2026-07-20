using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.OperationalAdministration;

public sealed record IssueOperationalTokenQuery(string Email) : IRequest<OperationalTokenResult>;

public sealed record OperationalTokenResult(
    string AccessToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Role);

public sealed class IssueOperationalTokenQueryValidator
    : AbstractValidator<IssueOperationalTokenQuery>
{
    public IssueOperationalTokenQueryValidator()
    {
        RuleFor(query => query.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public sealed class IssueOperationalTokenQueryHandler
    : IRequestHandler<IssueOperationalTokenQuery, OperationalTokenResult>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenIssuer _tokens;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public IssueOperationalTokenQueryHandler(
        IAppDbContext db,
        IJwtTokenIssuer tokens,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _tokens = tokens;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<OperationalTokenResult> Handle(
        IssueOperationalTokenQuery request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(
            item => item.Email.ToLower() == email,
            cancellationToken)
            ?? throw new NotFoundException("User not found.");
        if (user.Status != UserStatus.Active)
        {
            throw new ConflictException("Tokens can be issued only for active users.");
        }

        var (accessToken, expiresAt) = _tokens.Issue(user);
        await _audit.LogAsync(
            "OperationalTokenIssued",
            _currentUser.UserId,
            "User",
            user.Id,
            cancellationToken: cancellationToken);
        return new OperationalTokenResult(accessToken, expiresAt, user.Id, user.Role);
    }
}
