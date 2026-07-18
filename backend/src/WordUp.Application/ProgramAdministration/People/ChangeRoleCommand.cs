using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record ChangeRoleCommand(Guid Id, string Role) : IRequest;
