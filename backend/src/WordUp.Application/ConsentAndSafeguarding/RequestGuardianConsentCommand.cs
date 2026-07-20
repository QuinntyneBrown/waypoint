using MediatR;

namespace WordUp.Application.ConsentAndSafeguarding;

public sealed record RequestGuardianConsentCommand
    : IRequest<ConsentDeliveryResult>, IAtomicOutboxRequest;

public sealed record ConsentDeliveryResult(string Channel, string Destination);
