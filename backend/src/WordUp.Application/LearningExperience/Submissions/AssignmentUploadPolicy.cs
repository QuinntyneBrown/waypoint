using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Submissions;

public static class AssignmentUploadPolicy
{
    public const int MaxBytes = 10 * 1024 * 1024;

    internal static bool HasValidImageSignature(string contentType, byte[] content)
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
    string Url);

public sealed class StoreAssignmentUploadCommandValidator : AbstractValidator<StoreAssignmentUploadCommand>
{
    public StoreAssignmentUploadCommandValidator()
    {
        RuleFor(item => item.FileName).NotEmpty().MaximumLength(255);
        RuleFor(item => item.Content).NotEmpty().Must(content => content.Length <= AssignmentUploadPolicy.MaxBytes)
            .WithMessage("Image must be 10 MB or smaller.");
        RuleFor(item => item.ContentType)
            .Must(contentType => contentType is "image/png" or "image/jpeg")
            .WithMessage("Only JPG and PNG images are allowed.");
        RuleFor(item => item)
            .Must(item => AssignmentUploadPolicy.HasValidImageSignature(item.ContentType, item.Content))
            .WithMessage("The file content does not match a valid JPG or PNG image.");
        RuleFor(item => item)
            .Must(item =>
            {
                var extension = Path.GetExtension(Path.GetFileName(item.FileName)).ToLowerInvariant();
                return item.ContentType == "image/png" && extension == ".png"
                    || item.ContentType == "image/jpeg" && extension is ".jpg" or ".jpeg";
            })
            .WithMessage("The filename extension does not match the image type.");
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
        if (!AssignmentAccessGuard.SubmissionTypes(assignment).Contains("photo"))
        {
            throw new ForbiddenException("This assignment does not allow photo submissions.");
        }

        var file = new StoredSubmissionFile
        {
            OwnerUserId = youthId,
            AssignmentId = assignment.Id,
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
            $"/api/me/files/{file.Id}");
    }
}

public sealed record SubmissionFileQuery(Guid FileId) : IRequest<SubmissionFileView>;
public sealed record SubmissionFileView(string FileName, string ContentType, byte[] Content);

public sealed class SubmissionFileQueryHandler : IRequestHandler<SubmissionFileQuery, SubmissionFileView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmissionFileQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SubmissionFileView> Handle(SubmissionFileQuery request, CancellationToken cancellationToken)
    {
        var file = await _db.StoredSubmissionFiles.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.FileId, cancellationToken)
            ?? throw new NotFoundException("File not found.");
        if (file.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This file does not belong to you.");
        }
        return new SubmissionFileView(file.OriginalFileName, file.ContentType, file.Content);
    }
}
