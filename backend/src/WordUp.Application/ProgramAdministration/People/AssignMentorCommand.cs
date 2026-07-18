using MediatR;

namespace WordUp.Application.ProgramAdministration.People;

public record AssignMentorCommand(Guid YouthId, Guid MentorId) : IRequest;
