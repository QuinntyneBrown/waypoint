namespace Waypoint.Application.Shared.Abstractions;

/// <summary>
/// Signs and verifies the append-only audit chain (L2-CONSENT-003 AC3).
/// </summary>
public interface IAuditIntegrity
{
    string ComputeHash(AuditEntry entry);
    bool VerifyChain(IReadOnlyCollection<AuditEntry> entries);
}
