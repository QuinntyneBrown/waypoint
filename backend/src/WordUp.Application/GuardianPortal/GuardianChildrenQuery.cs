using MediatR;

namespace WordUp.Application.GuardianPortal;

/// <summary>Minimal linked-child selector for Guardian portal navigation.</summary>
public sealed record GuardianChildrenQuery : IRequest<IReadOnlyList<GuardianChildSummary>>;

public sealed record GuardianChildSummary(Guid ChildId, string ChildName);
