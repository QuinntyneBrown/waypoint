namespace Waypoint.Api.Contracts.PlatformServices;

public record FulfillDataSubjectRequest(
    string? DisplayName,
    string? Email,
    string? Pronouns,
    bool? ClearPronouns,
    string? ResolutionNote,
    bool VerifiedOwnershipAttested = false,
    IReadOnlyList<Guid>? VerifiedContactRequestIds = null,
    IReadOnlyList<Guid>? VerifiedVolunteerSignupIds = null,
    IReadOnlyList<Guid>? VerifiedDonationPledgeIds = null,
    IReadOnlyList<Guid>? VerifiedApplicantApplicationIds = null,
    IReadOnlyList<Guid>? VerifiedGuardianApplicationIds = null);
