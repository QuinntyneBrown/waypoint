using MediatR;

namespace WordUp.Application.ConsentAndSafeguarding;

public record WithdrawConsentCommand(Guid ConsentId) : IRequest;
