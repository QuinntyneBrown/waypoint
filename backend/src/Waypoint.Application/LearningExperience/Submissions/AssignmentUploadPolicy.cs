using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.ConsentAndSafeguarding;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.LearningExperience.Submissions;

public static class AssignmentUploadPolicy
{
    public const int MaxBytes = 10 * 1024 * 1024;

    internal static SubmissionFileKind Kind(string contentType) =>
        contentType == "application/pdf" ? SubmissionFileKind.File : SubmissionFileKind.Photo;

    internal static bool HasValidSignature(string contentType, byte[] content)
    {
        if (contentType == "image/png")
        {
            byte[] signature = [137, 80, 78, 71, 13, 10, 26, 10];
            return content.Length >= signature.Length && content.AsSpan(0, signature.Length).SequenceEqual(signature);
        }
        if (contentType == "image/jpeg")
        {
            return content.Length >= 3 && content[0] == 0xff && content[1] == 0xd8 && content[2] == 0xff;
        }
        if (contentType == "application/pdf")
        {
            byte[] signature = [0x25, 0x50, 0x44, 0x46, 0x2d];
            return content.Length >= signature.Length
                && content.AsSpan(0, signature.Length).SequenceEqual(signature);
        }
        return false;
    }
}

public sealed record StoreAssignmentUploadCommand(
    Guid AssignmentId,
    string FileName,
    string ContentType,
    byte[] Content) : IRequest<AssignmentUploadResult>;

public sealed record AssignmentUploadResult(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url,
    string Kind);

public sealed class StoreAssignmentUploadCommandValidator : AbstractValidator<StoreAssignmentUploadCommand>
{
    public StoreAssignmentUploadCommandValidator()
    {
        RuleFor(item => item.FileName).NotEmpty().MaximumLength(255);
        RuleFor(item => item.Content).NotEmpty().Must(content => content.Length <= AssignmentUploadPolicy.MaxBytes)
            .WithMessage("File must be 10 MB or smaller.");
        RuleFor(item => item.ContentType)
            .Must(contentType => contentType is "image/png" or "image/jpeg" or "application/pdf")
            .WithMessage("Only JPG, PNG, and PDF files are allowed.");
        RuleFor(item => item)
            .Must(item => AssignmentUploadPolicy.HasValidSignature(item.ContentType, item.Content))
            .WithMessage("Choose a valid PDF, JPG, or PNG file whose content matches its type.");
        RuleFor(item => item)
            .Must(item =>
            {
                var extension = Path.GetExtension(Path.GetFileName(item.FileName)).ToLowerInvariant();
                return item.ContentType == "image/png" && extension == ".png"
                    || item.ContentType == "image/jpeg" && extension is ".jpg" or ".jpeg"
                    || item.ContentType == "application/pdf" && extension == ".pdf";
            })
            .WithMessage("The filename extension does not match the file type.");
    }
}

public sealed class StoreAssignmentUploadCommandHandler
    : IRequestHandler<StoreAssignmentUploadCommand, AssignmentUploadResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public StoreAssignmentUploadCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AssignmentUploadResult> Handle(
        StoreAssignmentUploadCommand request,
        CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var assignment = await AssignmentAccessGuard.EnsureAccessibleAsync(
            _db, request.AssignmentId, youthId, cancellationToken);
        var kind = AssignmentUploadPolicy.Kind(request.ContentType);
        var allowedType = kind == SubmissionFileKind.Photo ? "photo" : "file";
        if (!AssignmentAccessGuard.SubmissionTypes(assignment).Contains(allowedType))
        {
            throw new ForbiddenException($"This assignment does not allow {allowedType} submissions.");
        }
        if (kind == SubmissionFileKind.Photo
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                youthId,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            throw new MediaConsentRequiredException();
        }

        var file = new StoredSubmissionFile
        {
            OwnerUserId = youthId,
            AssignmentId = assignment.Id,
            Kind = kind,
            OriginalFileName = Path.GetFileName(request.FileName),
            ContentType = request.ContentType,
            SizeBytes = request.Content.LongLength,
            Sha256 = Convert.ToHexString(SHA256.HashData(request.Content)).ToLowerInvariant(),
            Content = request.Content,
            CreatedAt = _clock.UtcNow,
        };
        _db.StoredSubmissionFiles.Add(file);
        await _db.SaveChangesAsync(cancellationToken);
        return new AssignmentUploadResult(
            file.Id,
            file.OriginalFileName,
            file.ContentType,
            file.SizeBytes,
            $"/api/submission-files/{file.Id}",
            allowedType);
    }
}

public sealed record SubmissionFileQuery(Guid FileId) : IRequest<SubmissionFileView>;
public sealed record SubmissionFileView(string FileName, string ContentType, byte[] Content);

public sealed class SubmissionFileQueryHandler : IRequestHandler<SubmissionFileQuery, SubmissionFileView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmissionFileQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<SubmissionFileView> Handle(SubmissionFileQuery request, CancellationToken cancellationToken)
    {
        var file = await _db.StoredSubmissionFiles.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.FileId, cancellationToken)
            ?? throw new NotFoundException("File not found.");
        var currentUserId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var canRead = file.OwnerUserId == currentUserId;
        if (!canRead && _currentUser.Role == Roles.ProgramAdministrator)
        {
            canRead = await IsLinkedToSubmissionAsync(file.Id, cancellationToken);
        }
        if (!canRead && _currentUser.Role == Roles.Mentor)
        {
            canRead = await (
                from submission in _db.Submissions.AsNoTracking()
                join youth in _db.Users.AsNoTracking() on submission.YouthUserId equals youth.Id
                where (submission.PhotoUploadId == file.Id || submission.FileUploadId == file.Id)
                    && youth.MentorId == currentUserId
                select submission.Id)
                .AnyAsync(cancellationToken);
        }
        if (!canRead)
        {
            throw new ForbiddenException("You cannot access this private submission file.");
        }
        if (file.Kind == SubmissionFileKind.Photo
            && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                file.OwnerUserId,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            throw new MediaConsentRequiredException();
        }
        return new SubmissionFileView(file.OriginalFileName, file.ContentType, file.Content);
    }

    private Task<bool> IsLinkedToSubmissionAsync(Guid fileId, CancellationToken cancellationToken) =>
        _db.Submissions.AsNoTracking().AnyAsync(
            item => item.PhotoUploadId == fileId || item.FileUploadId == fileId,
            cancellationToken);
}
