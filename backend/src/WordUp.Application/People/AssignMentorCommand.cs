using MediatR;

namespace WordUp.Application.People;

public record AssignMentorCommand(Guid YouthId, Guid MentorId) : IRequest;
