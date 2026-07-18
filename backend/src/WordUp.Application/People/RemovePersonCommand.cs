using MediatR;

namespace WordUp.Application.People;

public record RemovePersonCommand(Guid Id) : IRequest;
