namespace Waypoint.Api.Contracts.PublicSite;

public record DonationPledgeBody(
    string DonorName,
    string Email,
    string Tier,
    decimal? MonthlyAmount,
    string? Frequency,
    string? ProviderReference);
