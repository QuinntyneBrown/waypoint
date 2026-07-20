namespace WordUp.Application.Shared.Abstractions;

/// <summary>
/// Marks an application request whose persisted business changes and outbound-email envelopes must
/// share one database transaction.
/// </summary>
public interface IAtomicOutboxRequest : IAtomicRequest;
