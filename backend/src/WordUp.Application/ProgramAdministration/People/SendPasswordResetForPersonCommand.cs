using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record SendPasswordResetForPersonCommand(Guid Id) : IRequest;
