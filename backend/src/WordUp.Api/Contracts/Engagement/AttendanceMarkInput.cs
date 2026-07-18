namespace WordUp.Api.Contracts.Engagement;

public record AttendanceMarkInput(Guid YouthId, string Status, string? Note, string? ExpectedVersion = null);
