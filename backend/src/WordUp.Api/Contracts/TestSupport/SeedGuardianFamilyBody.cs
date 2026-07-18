namespace WordUp.Api.Contracts.TestSupport;

/// <summary>Testing-only request for a linked Guardian/child aggregate fixture.</summary>
public sealed record SeedGuardianFamilyBody(Guid GuardianUserId, bool IncludeConsent = true);
