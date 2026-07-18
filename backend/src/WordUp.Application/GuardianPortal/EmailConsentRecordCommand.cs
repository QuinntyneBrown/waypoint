using MediatR;

namespace WordUp.Application.GuardianPortal;

public sealed record EmailConsentRecordCommand(Guid ConsentId) : IRequest;
