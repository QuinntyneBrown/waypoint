namespace WordUp.Domain;

/// <summary>
/// A public donation pledge (L2-PUB-003 AC3). Payment is handled by an external PCI-compliant provider;
/// this record stores only the pledge intent and an opaque provider reference — NEVER card/PAN data.
/// </summary>
public class DonationPledge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DonorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Selected giving tier (e.g. "monthly-25"). Recurring-monthly is the current product decision.</summary>
    public string Tier { get; set; } = string.Empty;
    public decimal? MonthlyAmount { get; set; }
    public string Frequency { get; set; } = "monthly";

    /// <summary>Opaque token/reference from the external payment provider. No card data is ever stored.</summary>
    public string? ProviderReference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
