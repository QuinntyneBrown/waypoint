using MediatR;

namespace Waypoint.Application.ConsentAndSafeguarding;

public record WithdrawConsentCommand(Guid ConsentId) : IRequest;
