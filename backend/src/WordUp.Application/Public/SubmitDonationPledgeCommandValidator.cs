using FluentValidation;

namespace WordUp.Application.Public;

public class SubmitDonationPledgeCommandValidator : AbstractValidator<SubmitDonationPledgeCommand>
{
    public SubmitDonationPledgeCommandValidator()
    {
        RuleFor(x => x.DonorName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Tier).NotEmpty().MaximumLength(128);
        RuleFor(x => x.MonthlyAmount).GreaterThan(0m).When(x => x.MonthlyAmount.HasValue);
        RuleFor(x => x.ProviderReference).MaximumLength(256);
    }
}
