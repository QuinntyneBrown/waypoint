using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.PlatformServices.Files;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Support;

public sealed record StoreSupportScreenshotCommand(
    string FileName,
    string ContentType,
    byte[] Content) : IRequest<SupportScreenshotResult>;

public sealed record SupportScreenshotResult(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);

public sealed class StoreSupportScreenshotCommandValidator
    : AbstractValidator<StoreSupportScreenshotCommand>
{
    public StoreSupportScreenshotCommandValidator()
    {
        RuleFor(item => item.FileName).NotEmpty().MaximumLength(255);
        RuleFor(item => item.Content)
            .NotEmpty()
            .Must(content => content.Length <= SafeImageUploadPolicy.MaxBytes)
            .WithMessage("Support screenshot must be 5 MB or smaller.");
        RuleFor(item => item.ContentType)
            .Must(SafeImageUploadPolicy.IsSupportedContentType)
            .WithMessage("Only JPG and PNG support screenshots are allowed.");
        RuleFor(item => item)
            .Must(item => SafeImageUploadPolicy.ExtensionMatches(item.FileName, item.ContentType))
            .WithMessage("The filename extension does not match the image type.");
        RuleFor(item => item)
            .Must(item => SafeImageUploadPolicy.HasValidSignature(item.ContentType, item.Content))
            .WithMessage("Choose a valid JPG or PNG image whose content matches its type.");
    }
}

public sealed class StoreSupportScreenshotCommandHandler
    : IRequestHandler<StoreSupportScreenshotCommand, SupportScreenshotResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public StoreSupportScreenshotCommandHandler(
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

    public async Task<SupportScreenshotResult> Handle(
        StoreSupportScreenshotCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var screenshot = new StoredUserFile
        {
            OwnerUserId = userId,
            Purpose = StoredUserFilePurpose.SupportScreenshot,
            OriginalFileName = Path.GetFileName(request.FileName),
            ContentType = request.ContentType,
            SizeBytes = request.Content.LongLength,
            Sha256 = Convert.ToHexString(SHA256.HashData(request.Content)).ToLowerInvariant(),
            Content = request.Content,
            CreatedAt = _clock.UtcNow,
        };
        _db.StoredUserFiles.Add(screenshot);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "SupportScreenshotUploaded",
            userId,
            "StoredUserFile",
            screenshot.Id,
            cancellationToken: cancellationToken);

        return new SupportScreenshotResult(
            screenshot.Id,
            screenshot.OriginalFileName,
            screenshot.ContentType,
            screenshot.SizeBytes,
            $"/api/support/screenshots/{screenshot.Id}");
    }
}

public sealed record GetSupportScreenshotQuery(Guid Id) : IRequest<SupportScreenshotFile>;
public sealed record SupportScreenshotFile(string FileName, string ContentType, byte[] Content);

public sealed class GetSupportScreenshotQueryHandler
    : IRequestHandler<GetSupportScreenshotQuery, SupportScreenshotFile>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetSupportScreenshotQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SupportScreenshotFile> Handle(
        GetSupportScreenshotQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var screenshot = await _db.StoredUserFiles.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id
                && item.Purpose == StoredUserFilePurpose.SupportScreenshot, cancellationToken)
            ?? throw new NotFoundException("Support screenshot not found.");
        var authorized = screenshot.OwnerUserId == userId;
        if (!authorized && _currentUser.Role == Roles.ProgramAdministrator)
        {
            authorized = await _db.SupportRequests.AsNoTracking()
                .AnyAsync(item => item.ScreenshotFileId == screenshot.Id, cancellationToken);
        }
        if (!authorized)
        {
            throw new ForbiddenException("You cannot view this support screenshot.");
        }

        return new SupportScreenshotFile(
            screenshot.OriginalFileName,
            screenshot.ContentType,
            screenshot.Content);
    }
}

public sealed record RemoveSupportScreenshotCommand(Guid Id) : IRequest;

public sealed class RemoveSupportScreenshotCommandHandler
    : IRequestHandler<RemoveSupportScreenshotCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public RemoveSupportScreenshotCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task Handle(
        RemoveSupportScreenshotCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var screenshot = await _db.StoredUserFiles.FirstOrDefaultAsync(
            item => item.Id == request.Id
                && item.Purpose == StoredUserFilePurpose.SupportScreenshot,
            cancellationToken) ?? throw new NotFoundException("Support screenshot not found.");
        if (screenshot.OwnerUserId != userId)
        {
            throw new ForbiddenException("This support screenshot does not belong to you.");
        }
        if (await _db.SupportRequests.AsNoTracking()
            .AnyAsync(item => item.ScreenshotFileId == screenshot.Id, cancellationToken))
        {
            throw new ConflictException("A submitted support screenshot cannot be removed.");
        }

        _db.StoredUserFiles.Remove(screenshot);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "SupportScreenshotRemoved",
            userId,
            "StoredUserFile",
            screenshot.Id,
            cancellationToken: cancellationToken);
    }
}
