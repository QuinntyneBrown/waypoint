using FluentValidation;
using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public record CreateStageCommand(string Name, string? Description, int Order) : IRequest<StageResult>;

public sealed class CreateStageCommandValidator : AbstractValidator<CreateStageCommand>
{
    public CreateStageCommandValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(128);
        RuleFor(item => item.Description).MaximumLength(1000);
        RuleFor(item => item.Order).InclusiveBetween(0, 10000);
    }
}
