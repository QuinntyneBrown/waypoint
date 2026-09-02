using MediatR;

namespace Waypoint.Application.ProgramAdministration.People;

public record AssignMentorCommand(Guid YouthId, Guid MentorId) : IRequest;
