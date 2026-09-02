using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.ConsentAndSafeguarding;
using Waypoint.Application.PlatformServices.Files;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.PlatformServices.Profile;

public sealed record StoreProfilePhotoCommand(
    string FileName,
    string ContentType,
    byte[] Content) : IRequest<ProfilePhotoResult>;

public sealed record ProfilePhotoResult(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);

public sealed class StoreProfilePhotoCommandValidator : AbstractValidator<StoreProfilePhotoCommand>
{
    public StoreProfilePhotoCommandValidator()
    {
        RuleFor(item => item.FileName).NotEmpty().MaximumLength(255);
        RuleFor(item => item.Content)
            .NotEmpty()
            .Must(content => content.Length <= SafeImageUploadPolicy.MaxBytes)
            .WithMessage("Profile photo must be 5 MB or smaller.");
        RuleFor(item => item.ContentType)
            .Must(SafeImageUploadPolicy.IsSupportedContentType)
            .WithMessage("Only JPG and PNG profile photos are allowed.");
        RuleFor(item => item)
            .Must(item => SafeImageUploadPolicy.ExtensionMatches(item.FileName, item.ContentType))
            .WithMessage("The filename extension does not match the image type.");
        RuleFor(item => item)
            .Must(item => SafeImageUploadPolicy.HasValidSignature(item.ContentType, item.Content))
            .WithMessage("File content does not match a valid JPG or PNG image.");
    }
}

public sealed class StoreProfilePhotoCommandHandler
    : IRequestHandler<StoreProfilePhotoCommand, ProfilePhotoResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public StoreProfilePhotoCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
    }

    public async Task<ProfilePhotoResult> Handle(
        StoreProfilePhotoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        if (user.Role == Roles.Youth
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                user.Id,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            throw new MediaConsentRequiredException();
        }

        var previous = await _db.StoredUserFiles
            .Where(item => item.OwnerUserId == userId
                && item.Purpose == StoredUserFilePurpose.ProfilePhoto)
            .ToListAsync(cancellationToken);
        _db.StoredUserFiles.RemoveRange(previous);

        var file = new StoredUserFile
        {
            OwnerUserId = userId,
            Purpose = StoredUserFilePurpose.ProfilePhoto,
            OriginalFileName = Path.GetFileName(request.FileName),
            ContentType = request.ContentType,
            SizeBytes = request.Content.LongLength,
            Sha256 = Convert.ToHexString(SHA256.HashData(request.Content)).ToLowerInvariant(),
            Content = request.Content,
            CreatedAt = _clock.UtcNow,
        };
        _db.StoredUserFiles.Add(file);
        user.PhotoUrl = $"/api/profile-photos/{file.Id}";
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ProfilePhotoUpdated", userId, "StoredUserFile", file.Id, cancellationToken: cancellationToken);

        return new ProfilePhotoResult(
            file.Id,
            file.OriginalFileName,
            file.ContentType,
            file.SizeBytes,
            user.PhotoUrl);
    }
}

public sealed record GetMyProfileQuery : IRequest<MyProfileView>;

public sealed record MyProfileView(
    string DisplayName,
    string Email,
    string? Pronouns,
    string? PhotoUrl,
    bool HasPhoto);

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public GetMyProfileQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<MyProfileView> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        var photo = await _db.StoredUserFiles.AsNoTracking()
            .Where(item => item.OwnerUserId == userId
                && item.Purpose == StoredUserFilePurpose.ProfilePhoto)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var photoAllowed = photo is not null
            && (user.Role != Roles.Youth
                || await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                    _db,
                    user.Id,
                    YouthMediaUse.IndividualPhotoOrVideo,
                    _clock.UtcNow,
                    cancellationToken));

        return new MyProfileView(
            user.DisplayName,
            user.Email,
            user.Pronouns,
            photoAllowed ? $"/api/profile-photos/{photo!.Id}" : null,
            photo is not null);
    }
}

public sealed record GetProfilePhotoQuery(Guid Id) : IRequest<ProfilePhotoFile>;

public sealed record ProfilePhotoFile(string ContentType, byte[] Content);

public sealed class GetProfilePhotoQueryHandler : IRequestHandler<GetProfilePhotoQuery, ProfilePhotoFile>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public GetProfilePhotoQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<ProfilePhotoFile> Handle(
        GetProfilePhotoQuery request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var file = await _db.StoredUserFiles.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id
                && item.Purpose == StoredUserFilePurpose.ProfilePhoto, cancellationToken)
            ?? throw new NotFoundException("Profile photo not found.");
        var owner = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == file.OwnerUserId, cancellationToken)
            ?? throw new NotFoundException("Profile owner not found.");

        var authorized = actorId == owner.Id
            || _currentUser.Role == Roles.ProgramAdministrator
            || (_currentUser.Role == Roles.Mentor && owner.MentorId == actorId)
            || (_currentUser.Role == Roles.Guardian && owner.GuardianUserId == actorId);
        if (!authorized)
        {
            throw new ForbiddenException("You cannot view this profile photo.");
        }
        if (owner.Role == Roles.Youth
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                owner.Id,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            throw new MediaConsentRequiredException();
        }

        return new ProfilePhotoFile(file.ContentType, file.Content);
    }
}

public sealed record RemoveProfilePhotoCommand : IRequest;

public sealed class RemoveProfilePhotoCommandHandler : IRequestHandler<RemoveProfilePhotoCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public RemoveProfilePhotoCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task Handle(RemoveProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        var files = await _db.StoredUserFiles
            .Where(item => item.OwnerUserId == userId
                && item.Purpose == StoredUserFilePurpose.ProfilePhoto)
            .ToListAsync(cancellationToken);
        _db.StoredUserFiles.RemoveRange(files);
        user.PhotoUrl = null;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ProfilePhotoRemoved", userId, "User", userId, cancellationToken: cancellationToken);
    }
}
