using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record LinkGuardianCommand(Guid YouthId, Guid GuardianId) : IRequest;
