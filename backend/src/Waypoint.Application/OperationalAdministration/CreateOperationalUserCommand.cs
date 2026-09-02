using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.OperationalAdministration;

public sealed record CreateOperationalUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string Role,
    Guid? CohortId = null,
    Guid? MentorId = null) : IRequest<OperationalUserResult>;

public sealed record OperationalUserResult(Guid Id, string Email, string DisplayName, string Role);

public sealed class CreateOperationalUserCommandValidator
    : AbstractValidator<CreateOperationalUserCommand>
{
    public CreateOperationalUserCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Password)
            .NotEmpty()
            .MaximumLength(128)
            .Must(PasswordPolicy.IsCompliant)
            .WithMessage(PasswordPolicy.Message);
        RuleFor(command => command.DisplayName).NotEmpty().MaximumLength(128);
        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(Roles.IsValid)
            .WithMessage("Unknown role.");
    }
}

public sealed class CreateOperationalUserCommandHandler
    : IRequestHandler<CreateOperationalUserCommand, OperationalUserResult>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwords;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateOperationalUserCommandHandler(
        IAppDbContext db,
        IPasswordHasher passwords,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _passwords = passwords;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<OperationalUserResult> Handle(
        CreateOperationalUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(user => user.Email.ToLower() == email, cancellationToken))
        {
            throw new ConflictException("A user with this email already exists.");
        }

        if (request.CohortId is Guid cohortId
            && !await _db.Cohorts.AnyAsync(cohort => cohort.Id == cohortId, cancellationToken))
        {
            throw new ConflictException("Cohort not found.");
        }

        if (request.MentorId is Guid mentorId
            && !await _db.Users.AnyAsync(
                user => user.Id == mentorId
                    && user.Role == Roles.Mentor
                    && user.Status == UserStatus.Active,
                cancellationToken))
        {
            throw new ConflictException("Choose an active mentor.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwords.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Role = request.Role,
            Status = UserStatus.Active,
            CohortId = request.CohortId,
            MentorId = request.MentorId,
            CreatedAt = _clock.UtcNow,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "UserCreated",
            _currentUser.UserId,
            "User",
            user.Id,
            cancellationToken: cancellationToken);

        return new OperationalUserResult(user.Id, user.Email, user.DisplayName, user.Role);
    }
}
