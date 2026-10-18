using FluentValidation;
using MediatR;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record StartVideoUploadCommand(Guid LessonId, string FileName, long SizeBytes)
    : IRequest<VideoUploadSessionResult>;

public sealed record VideoUploadSessionResult(Guid UploadId, int ChunkSizeBytes, int ChunkCount);

public sealed class StartVideoUploadCommandValidator : AbstractValidator<StartVideoUploadCommand>
{
    public StartVideoUploadCommandValidator(IMediaSettings media)
    {
        RuleFor(item => item.LessonId).NotEmpty();
        RuleFor(item => item.FileName).NotEmpty().MaximumLength(255)
            .Must(LessonVideoUploadPolicy.HasVideoExtension)
            .WithMessage("Only MP4 videos are allowed.");
        RuleFor(item => item.SizeBytes).GreaterThan(0)
            .LessThanOrEqualTo(media.MaxSourceBytes)
            .WithMessage("The video is larger than the configured size limit.");
    }
}
