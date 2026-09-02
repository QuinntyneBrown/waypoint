using FluentValidation;
using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Progression;

public sealed record UpdateStageCommand(Guid StageId, string Name, string? Description) : IRequest;

public sealed class UpdateStageCommandValidator : AbstractValidator<UpdateStageCommand>
{
    public UpdateStageCommandValidator()
    {
        RuleFor(item => item.StageId).NotEmpty();
        RuleFor(item => item.Name).NotEmpty().MaximumLength(128);
        RuleFor(item => item.Description).MaximumLength(1000);
    }
}
