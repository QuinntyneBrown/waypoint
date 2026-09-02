using FluentValidation;
using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record UpdateLessonCommand(
    Guid LessonId,
    Guid ModuleId,
    string Title,
    string? ScriptureReference,
    int? EstimatedMinutes,
    string? LearningObjectives,
    string? TeachingBody,
    string? ReflectionPrompts,
    string? LeaderGuideNotes) : IRequest;

public sealed class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(item => item.LessonId).NotEmpty();
        RuleFor(item => item.ModuleId).NotEmpty();
        RuleFor(item => item.Title).NotEmpty().MaximumLength(200);
        RuleFor(item => item.EstimatedMinutes).InclusiveBetween(1, 600)
            .When(item => item.EstimatedMinutes.HasValue);
    }
}
