using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record ReactivatePersonCommand(Guid Id) : IRequest;
