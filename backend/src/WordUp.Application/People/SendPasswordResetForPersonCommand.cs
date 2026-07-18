using MediatR;

namespace WordUp.Application.People;

public record SendPasswordResetForPersonCommand(Guid Id) : IRequest;
