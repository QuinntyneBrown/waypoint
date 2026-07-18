using MediatR;

namespace WordUp.Application.Consent;

public record WithdrawConsentCommand(Guid ConsentId) : IRequest;
