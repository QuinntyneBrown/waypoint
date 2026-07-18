using MediatR;

namespace WordUp.Application.Public;

/// <summary>
/// A public donation pledge (L2-PUB-003 AC3). Payment is handled by an external PCI-compliant provider;
/// only the pledge intent (tier/amount/frequency) plus an optional opaque provider reference is stored —
/// NEVER card/PAN data.
/// </summary>
public record SubmitDonationPledgeCommand(
    string DonorName,
    string Email,
    string Tier,
    decimal? MonthlyAmount,
    string? Frequency,
    string? ProviderReference) : IRequest<SubmitDonationPledgeResult>;
