using MediatR;

namespace WordUp.Application.PlatformServices.Privacy;

/// <summary>
/// Records a data-subject request — access, correction, removal, or consent-withdrawal — for the current
/// authenticated user (L2-NFR-003 AC2). For the "access" right the caller reads their own data via
/// <see cref="MyDataQuery"/> while the durable request remains available for administrator fulfilment.
/// </summary>
public sealed record DataSubjectCorrection(
    string? DisplayName,
    string? Email,
    string? Pronouns,
    bool ClearPronouns = false);

public record RequestDataSubjectActionCommand(
    string ActionType,
    string? Details,
    DataSubjectCorrection? Correction)
    : IRequest<DataSubjectRequestResult>;

public record DataSubjectRequestResult(
    Guid Id,
    string Reference,
    Guid SubjectUserId,
    string ActionType,
    string Status,
    DateTime RequestedAt)
{
    public DateTime RecordedAt => RequestedAt;
    public string PublicSubmissionScopeNotice => PrivacyScopeNotices.UnlinkedPublicSubmissions;
}

public static class PrivacyScopeNotices
{
    public const string AccountLinkedPublicSubmissions =
        "Includes public-form submissions made while authenticated and explicitly linked to this account.";

    public const string UnlinkedPublicSubmissions =
        "Anonymous public-form and application records are not claimed from email equality alone; an administrator must separately verify their record IDs and attest the links during DSR fulfillment before they can be exported, corrected, or erased.";
}
