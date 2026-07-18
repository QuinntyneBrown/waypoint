using MediatR;

namespace WordUp.Application.People;

public record ReactivatePersonCommand(Guid Id) : IRequest;
