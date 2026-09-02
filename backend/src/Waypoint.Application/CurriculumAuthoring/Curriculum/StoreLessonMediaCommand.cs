using FluentValidation;
using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record StoreLessonMediaCommand(
    Guid LessonId,
    string FileName,
    string ContentType,
    byte[] Content) : IRequest<LessonMediaResult>;

public sealed record LessonMediaResult(
    Guid Id,
    string MediaType,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);

public sealed class StoreLessonMediaCommandValidator : AbstractValidator<StoreLessonMediaCommand>
{
    public StoreLessonMediaCommandValidator()
    {
        RuleFor(item => item.LessonId).NotEmpty();
        RuleFor(item => item.FileName).NotEmpty().MaximumLength(255);
        RuleFor(item => item.Content).NotEmpty()
            .Must(content => content.Length <= LessonMediaUploadPolicy.MaxBytes)
            .WithMessage("Lesson media must be 25 MB or smaller.");
        RuleFor(item => item.ContentType)
            .Must(contentType => contentType is "application/pdf" or "video/mp4")
            .WithMessage("Only PDF documents and MP4 videos are allowed.");
        RuleFor(item => item)
            .Must(item => LessonMediaUploadPolicy.ExtensionMatches(item.FileName, item.ContentType))
            .WithMessage("The filename extension does not match the media type.");
        RuleFor(item => item)
            .Must(item => LessonMediaUploadPolicy.HasValidSignature(item.ContentType, item.Content))
            .WithMessage("The file content does not match a valid PDF or MP4 file.");
    }
}
