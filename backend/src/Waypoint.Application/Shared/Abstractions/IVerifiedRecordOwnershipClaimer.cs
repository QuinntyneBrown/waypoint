namespace Waypoint.Application.Shared.Abstractions;

public interface IVerifiedRecordOwnershipClaimer
{
    Task<int> ClaimAsync(
        Guid subjectUserId,
        IReadOnlyList<Guid> contactRequestIds,
        IReadOnlyList<Guid> volunteerSignupIds,
        IReadOnlyList<Guid> donationPledgeIds,
        IReadOnlyList<Guid> applicantApplicationIds,
        IReadOnlyList<Guid> guardianApplicationIds,
        CancellationToken cancellationToken = default);
}
