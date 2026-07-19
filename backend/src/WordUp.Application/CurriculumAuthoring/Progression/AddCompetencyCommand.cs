using FluentValidation;
using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public record AddCompetencyCommand(Guid StageId, string Name) : IRequest<CompetencyResult>;

public sealed class AddCompetencyCommandValidator : AbstractValidator<AddCompetencyCommand>
{
    public AddCompetencyCommandValidator()
    {
        RuleFor(item => item.StageId).NotEmpty();
        RuleFor(item => item.Name).NotEmpty().MaximumLength(128);
    }
}
