using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record DeactivatePersonCommand(Guid Id) : IRequest;
