using MediatR;

namespace Waypoint.Application.GuardianPortal;

public sealed record EmailConsentRecordCommand(Guid ConsentId) : IRequest, IAtomicOutboxRequest;
