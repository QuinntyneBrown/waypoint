using MediatR;

namespace WordUp.Application.People;

public record ChangeRoleCommand(Guid Id, string Role) : IRequest;
