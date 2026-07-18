using MediatR;

namespace WordUp.Application.People;

public record DeactivatePersonCommand(Guid Id) : IRequest;
