using FluentValidation;
using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record UpdateModuleCommand(
    Guid ModuleId,
    Guid TrackId,
    string Name,
    string? Description,
    Guid? PrerequisiteModuleId,
    string? ScriptureAnchor) : IRequest;

public sealed class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(item => item.ModuleId).NotEmpty();
        RuleFor(item => item.TrackId).NotEmpty();
        RuleFor(item => item.Name).NotEmpty().MaximumLength(128);
    }
}
