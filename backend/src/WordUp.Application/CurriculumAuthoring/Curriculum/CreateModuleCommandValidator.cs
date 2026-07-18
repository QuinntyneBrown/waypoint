using FluentValidation;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
